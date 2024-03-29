﻿using Infrastructure.Authentication.Models;
using AutoMapper;
using Infrastructure.Exceptions;
using SupportTickets.Api.Models.Messages;
using SupportTickets.Api.Models.Solutions;
using SupportTickets.Api.Models.SupportTicketAgentRecords;
using SupportTickets.Api.Models.SupportTickets;
using SupportTickets.Api.Models.SupportTicketsPages;
using SupportTickets.Api.Models.SupportTicketStatusRecords;
using SupportTickets.Api.Models.Users;
using SupportTickets.Api.Repositories.Messages;
using SupportTickets.Api.Repositories.Solutions;
using SupportTickets.Api.Repositories.SupportTicketAgentRecords;
using SupportTickets.Api.Repositories.SupportTickets;
using SupportTickets.Api.Repositories.SupportTicketStatusRecords;
using SupportTickets.Api.Services.Clients;
using SupportTickets.Api.Services.JobsManagers.Closing;
using SupportTickets.Api.Services.JobsManagers.Escalations;
using SupportTickets.Api.Services.SupportTicketsPaginationQueueBuilder;
using static SupportTickets.Api.Constants.PermissionNames.SupportTicketPermissions;

namespace SupportTickets.Api.Services;

public class SupportTicketsService
{
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly ISolutionsRepository _solutionsRepository;
    private readonly ISupportTicketAgentRecordsRepository _agentRecordsRepository;
    private readonly ISupportTicketStatusRecordsRepository _statusRecordsRepository;
    private readonly IResourcesGrpcClient _resourcesGrpcClient;
    private readonly ISupportTicketsEscalationManager _escalationManager;
    private readonly ISupportTicketsClosingManager _closingManager;
    private readonly IMapper _mapper;

    private static readonly TimeSpan TimeToEscalation = TimeSpan.FromDays(10);
    private static readonly TimeSpan TimeToCloseIfNotResponse = TimeSpan.FromDays(1);
    private static readonly TimeSpan TimeForReopen = TimeSpan.FromDays(10);

    public SupportTicketsService(
        ISupportTicketsRepository supportTicketsRepository,
        IMessagesRepository messagesRepository,
        ISolutionsRepository solutionsRepository,
        ISupportTicketAgentRecordsRepository agentRecordsRepository,
        ISupportTicketStatusRecordsRepository statusRecordsRepository,
        IResourcesGrpcClient resourcesGrpcClient,
        ISupportTicketsEscalationManager escalationManager,
        ISupportTicketsClosingManager closingManager,
        IMapper mapper)
    {
        _supportTicketsRepository = supportTicketsRepository;
        _messagesRepository = messagesRepository;
        _solutionsRepository = solutionsRepository;
        _agentRecordsRepository = agentRecordsRepository;
        _statusRecordsRepository = statusRecordsRepository;
        _resourcesGrpcClient = resourcesGrpcClient;
        _escalationManager = escalationManager;
        _closingManager = closingManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<SupportTicketPageView> GetAllPageAsync(SupportTicketPageGet pageGet)
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetByPagination(builder =>
        {
            SetFilterForGetAllPage(pageGet, builder);

            builder.LimitAndOffset(
                limit: pageGet.PageSize,
                offset: pageGet.PageSize * pageGet.PageNumber - pageGet.PageSize);
        });

        var pageView = _mapper.Map<SupportTicketPageView>(pageGet);
        pageView.SupportTickets = _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
        int count = await _supportTicketsRepository.GetCountByPagination(builder =>
        {
            SetFilterForGetAllPage(pageGet, builder);
        });
        pageView.TotalPages = (int)Math.Ceiling((double)count / pageGet.PageSize);
        return pageView;
    }

    private static void SetFilterForGetAllPage(
        SupportTicketPageGet pageGet,
        ISupportTicketsPaginationQueueBuilder builder)
    {
        if (pageGet.Status != null)
            builder.WhereStatusEquals(pageGet.Status.Value);

        if (pageGet.Priority != null)
            builder.WherePriorityEquals(pageGet.Priority.Value);
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetFreeAsync(Guid accountId)
    {
        IEnumerable<SupportTicket> allFreeSupportTickets = await _supportTicketsRepository.GetAllOpenWithoutAgent();

        var availableSupportTickets = new List<SupportTicket>();
        foreach (SupportTicket supportTicket in allFreeSupportTickets)
        {
            IEnumerable<SupportTicketAgentRecord> agentRecords =
                await _agentRecordsRepository.GetBySupportTicketIdAsync(supportTicket.Id);

            if (agentRecords.Any(_ => _.AgentId == accountId))
                continue;

            availableSupportTickets.Add(supportTicket);
        }

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(availableSupportTickets);
    }

    public async Task<SupportTicketPageView> GetFreePageAsync(
        SupportTicketPageGetFree pageGetFree,
        Guid accountId)
    {
        IEnumerable<SupportTicket> freeSupportTickets = await _supportTicketsRepository.GetByPagination(builder =>
        {
            SetFilterForGetFreePage(pageGetFree, builder);
            builder.LimitAndOffset(
                limit: pageGetFree.PageSize,
                offset: pageGetFree.PageSize * pageGetFree.PageNumber - pageGetFree.PageSize);
        });

        var availableSupportTickets = new List<SupportTicket>();
        foreach (SupportTicket supportTicket in freeSupportTickets)
        {
            IEnumerable<SupportTicketAgentRecord> agentRecords =
                await _agentRecordsRepository.GetBySupportTicketIdAsync(supportTicket.Id);

            if (agentRecords.Any(_ => _.AgentId == accountId))
                continue;

            availableSupportTickets.Add(supportTicket);
        }

        var pageView = _mapper.Map<SupportTicketPageView>(pageGetFree);
        pageView.Status = SupportTicketStatus.Open;
        pageView.SupportTickets = _mapper.Map<IEnumerable<SupportTicketPreview>>(availableSupportTickets);
        int count = await _supportTicketsRepository.GetCountByPagination(builder =>
        {
            SetFilterForGetFreePage(pageGetFree, builder);
        });
        pageView.TotalPages = (int)Math.Ceiling((double)count / pageGetFree.PageSize);
        return pageView;
    }

    private static void SetFilterForGetFreePage(
        SupportTicketPageGetFree pageGetFree,
        ISupportTicketsPaginationQueueBuilder builder)
    {
        if (pageGetFree.Priority != null)
            builder.WherePriorityEquals(pageGetFree.Priority.Value);

        builder
            .WhereStatusEquals(SupportTicketStatus.Open)
            .WhereAgentIdIsNull();
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetByAccountIdAsync(Guid accountId)
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetBasedOnAccountAsync(accountId);

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<SupportTicketPageView> GetByAccountIdPageAsync(
        SupportTicketPageGet pageGet,
        Guid accountId)
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetByPagination(builder =>
        {
            SetFilterGetByAccountIdPage(pageGet, accountId, builder);
            builder.LimitAndOffset(
                limit: pageGet.PageSize,
                offset: pageGet.PageSize * pageGet.PageNumber - pageGet.PageSize);
        });

        var pageView = _mapper.Map<SupportTicketPageView>(pageGet);
        pageView.SupportTickets = _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
        int count = await _supportTicketsRepository.GetCountByPagination(builder =>
        {
            SetFilterGetByAccountIdPage(pageGet, accountId, builder);
        });
        pageView.TotalPages = (int)Math.Ceiling((double)count / pageGet.PageSize);
        return pageView;
    }

    private static void SetFilterGetByAccountIdPage(
        SupportTicketPageGet pageGet,
        Guid accountId,
        ISupportTicketsPaginationQueueBuilder builder)
    {
        if (pageGet.Status != null)
            builder.WhereStatusEquals(pageGet.Status.Value);

        if (pageGet.Priority != null)
            builder.WherePriorityEquals(pageGet.Priority.Value);

        builder.WhereUserIdOrAgentIdEquals(accountId);
    }

    public async Task<SupportTicketView> GetByIdAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketId);

        if (!IsAccountRelatedToSupportTicket(supportTicket, account.Id) && !account.HasPermission(GetById))
            throw new UnauthorizedException();

        var supportTicketView = _mapper.Map<SupportTicketView>(supportTicket);
        if (supportTicketView.Messages != null)
        {
            foreach (MessageView messageView in supportTicketView.Messages)
            {
                messageView.Images = await _resourcesGrpcClient.SendGetMessageImagesRequestAsync(messageView.Id);
            }
        }

        return supportTicketView;
    }

    public async Task<Guid> CreateAsync(
        SupportTicketCreate supportTicketCreate,
        Account<Guid> account)
    {
        var supportTicket = _mapper.Map<SupportTicket>(supportTicketCreate);
        supportTicket.Id = Guid.NewGuid();
        supportTicket.User = _mapper.Map<User>(account);
        supportTicket.Status = SupportTicketStatus.Open;

        await _supportTicketsRepository.InsertAsync(supportTicket);

        var statusRecord = _mapper.Map<SupportTicketStatusRecord>(supportTicket);
        statusRecord.DateTime = DateTime.Now;
        await _statusRecordsRepository.InsertAsync(statusRecord);

        return supportTicket.Id;
    }

    public async Task UpdateAsync(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketUpdate.Id);

        if (supportTicket.User.Id != account.Id && !account.HasPermission(Update))
            throw new UnauthorizedException();

        ThrowIfSupportTicketNotOpen(supportTicket);
        ThrowIfSupportTicketHaveSuggestedSolution(supportTicket);

        var supportTicketUpdated = _mapper.Map<SupportTicket>(supportTicketUpdate);
        supportTicketUpdated.Agent = supportTicket.Agent;
        supportTicketUpdated.Status = supportTicket.Status;
        supportTicketUpdated.Priority = supportTicket.Priority;

        await _supportTicketsRepository.UpdateAsync(supportTicketUpdated);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _supportTicketsRepository.IsExistsAsync(id))
            throw new NotFoundException($"SupportTicket with id: {id} not found");

        await _supportTicketsRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SupportTicketAgentRecordView>> GetAgentHistoryAsync(Guid id)
    {
        IEnumerable<SupportTicketAgentRecord> records = await _agentRecordsRepository.GetBySupportTicketIdAsync(id);
        return _mapper.Map<IEnumerable<SupportTicketAgentRecordView>>(records);
    }

    public async Task AppointAgentAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketId);

        ThrowIfSupportTicketNotOpen(supportTicket);

        if (supportTicket.Agent != null)
            throw new BadRequestException($"SupportTicket with id: {supportTicketId} already have agent");

        IEnumerable<SupportTicketAgentRecord> agentRecords =
            await _agentRecordsRepository.GetBySupportTicketIdAsync(supportTicketId);

        if (agentRecords.Any(_ => _.AgentId == account.Id))
            throw new BadRequestException(
                $"Account with id: {account.Id} was already agent for SupportTicket with id: {supportTicketId}");

        supportTicket.Agent = _mapper.Map<User>(account);
        supportTicket.Priority = SupportTicketPriority.Low;
        await _supportTicketsRepository.UpdateAsync(supportTicket);

        var agentRecord = _mapper.Map<SupportTicketAgentRecord>(supportTicket);
        agentRecord.DateTime = DateTime.Now;
        await _agentRecordsRepository.InsertAsync(agentRecord);

        _escalationManager.AssignEscalationFor(supportTicketId, TimeToEscalation);
    }

    public async Task<Guid> AddMessageAsync(MessageCreate messageCreate, Account<Guid> account)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(messageCreate.SupportTicketId);

        if (!IsAccountRelatedToSupportTicket(supportTicket, account.Id))
            throw new UnauthorizedException();

        ThrowIfSupportTicketNotOpen(supportTicket);

        var message = _mapper.Map<Message>(messageCreate);
        message.Id = Guid.NewGuid();
        message.User = _mapper.Map<User>(account);
        message.DateTime = DateTime.Now;
        await _messagesRepository.InsertAsync(message);

        if (messageCreate.Images != null && messageCreate.Images.Any())
            await _resourcesGrpcClient.SendAddImagesToMessageRequestAsync(messageCreate.Images, message.Id);

        if (supportTicket.Agent?.Id == account.Id)
        {
            _closingManager.EnsureAssignCloseFor(messageCreate.SupportTicketId, TimeToCloseIfNotResponse);
        }
        else
        {
            _closingManager.EnsureCancelCloseFor(messageCreate.SupportTicketId);
        }

        return message.Id;
    }

    public async Task UpdateMessageAsync(MessageUpdate messageUpdate, Guid accountId)
    {
        Message message = await _messagesRepository.GetByIdAsync(messageUpdate.Id)
                          ?? throw new NotFoundException($"Message with id: {messageUpdate.Id} not found");

        if (message.User.Id != accountId)
            throw new UnauthorizedException();

        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(message.SupportTicketId);

        ThrowIfSupportTicketNotOpen(supportTicket);
        ThrowIfSupportTicketHaveSuggestedSolution(supportTicket);

        await _messagesRepository.UpdateAsync(_mapper.Map<Message>(messageUpdate));
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid accountId)
    {
        Message message = await _messagesRepository.GetByIdAsync(messageId)
                          ?? throw new NotFoundException($"Message with id: {messageId} not found");

        if (message.User.Id != accountId)
            throw new UnauthorizedException();

        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(message.SupportTicketId);

        ThrowIfSupportTicketNotOpen(supportTicket);
        ThrowIfSupportTicketHaveSuggestedSolution(supportTicket);

        await _messagesRepository.DeleteAsync(messageId);
    }

    public async Task<IEnumerable<SupportTicketStatusRecordView>> GetStatusHistoryAsync(Guid id)
    {
        IEnumerable<SupportTicketStatusRecord> records = await _statusRecordsRepository.GetBySupportTicketIdAsync(id);
        return _mapper.Map<IEnumerable<SupportTicketStatusRecordView>>(records);
    }

    public async Task SuggestSolutionAsync(SolutionSuggest solutionSuggest, Guid accountId)
    {
        Message message = await _messagesRepository.GetByIdAsync(solutionSuggest.MessageId)
                          ?? throw new NotFoundException($"Message with id: {solutionSuggest.MessageId} not found");

        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(message.SupportTicketId);

        if (supportTicket.Agent?.Id != accountId)
            throw new UnauthorizedException();

        ThrowIfSupportTicketNotOpen(supportTicket);
        ThrowIfSupportTicketHaveSuggestedSolution(supportTicket);

        if (supportTicket.Solutions.Any(_ => _.MessageId == solutionSuggest.MessageId))
            throw new BadRequestException($"Message with id: {solutionSuggest.MessageId} was already solution");

        var solution = _mapper.Map<Solution>(solutionSuggest);
        solution.Status = SolutionStatus.Suggested;
        await _solutionsRepository.InsertAsync(solution);

        _closingManager.EnsureAssignCloseFor(message.SupportTicketId, TimeToCloseIfNotResponse);
    }

    public async Task AcceptSolutionAsync(Guid supportTicketId, Guid accountId)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketId);

        if (supportTicket.User.Id != accountId)
            throw new UnauthorizedException();

        Solution solution = supportTicket.Solutions.FirstOrDefault(_ => _.Status == SolutionStatus.Suggested)
                            ?? throw new BadRequestException("Suggested solution not exists");

        solution.Status = SolutionStatus.Accepted;
        await _solutionsRepository.UpdateAsync(solution);

        supportTicket.Status = SupportTicketStatus.Solved;
        await _supportTicketsRepository.UpdateAsync(supportTicket);

        var statusRecord = _mapper.Map<SupportTicketStatusRecord>(supportTicket);
        statusRecord.DateTime = DateTime.Now;
        await _statusRecordsRepository.InsertAsync(statusRecord);

        _escalationManager.CancelEscalationFor(supportTicketId);
        _closingManager.EnsureCancelCloseFor(supportTicketId);
    }

    public async Task RejectSolutionAsync(Guid supportTicketId, Guid accountId)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketId);

        if (supportTicket.User.Id != accountId)
            throw new UnauthorizedException();

        Solution solution = supportTicket.Solutions.FirstOrDefault(_ => _.Status == SolutionStatus.Suggested)
                            ?? throw new BadRequestException("Suggested solution not exists");

        solution.Status = SolutionStatus.Rejected;
        await _solutionsRepository.UpdateAsync(solution);

        _closingManager.EnsureCancelCloseFor(supportTicketId);
    }

    public async Task CloseAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketId);

        if (supportTicket.Agent?.Id != account.Id && !account.HasPermission(Close))
            throw new UnauthorizedException();

        ThrowIfSupportTicketNotOpen(supportTicket);

        supportTicket.Status = SupportTicketStatus.Close;
        await _supportTicketsRepository.UpdateAsync(supportTicket);

        var statusRecord = _mapper.Map<SupportTicketStatusRecord>(supportTicket);
        statusRecord.DateTime = DateTime.Now;
        await _statusRecordsRepository.InsertAsync(statusRecord);

        _escalationManager.CancelEscalationFor(supportTicketId);
        _closingManager.EnsureCancelCloseFor(supportTicketId);
    }

    public async Task ReopenAsync(Guid supportTicketId, Guid accountId)
    {
        SupportTicket supportTicket = await GetSupportTicketOrThrowAsync(supportTicketId);

        if (supportTicket.User.Id != accountId)
            throw new UnauthorizedException();

        if (supportTicket.Status != SupportTicketStatus.Close)
            throw new BadRequestException($"SupportTicket with id {supportTicketId} not close");

        IEnumerable<SupportTicketStatusRecord> statusRecords =
            await _statusRecordsRepository.GetBySupportTicketIdAsync(supportTicketId);

        if (statusRecords.Last().DateTime.Add(TimeForReopen) < DateTime.Now)
            throw new BadRequestException($"SupportTicket can be reopen only if less elapsed than {TimeForReopen}");

        supportTicket.Status = SupportTicketStatus.Open;
        await _supportTicketsRepository.UpdateAsync(supportTicket);

        var statusRecord = _mapper.Map<SupportTicketStatusRecord>(supportTicket);
        statusRecord.DateTime = DateTime.Now;
        await _statusRecordsRepository.InsertAsync(statusRecord);

        _escalationManager.AssignEscalationFor(supportTicketId, TimeToEscalation);
    }

    private async Task<SupportTicket> GetSupportTicketOrThrowAsync(Guid supportTicketId)
    {
        return await _supportTicketsRepository.GetByIdAsync(supportTicketId)
               ?? throw new NotFoundException($"SupportTicket with id: {supportTicketId} not found");
    }

    private static void ThrowIfSupportTicketNotOpen(SupportTicket supportTicket)
    {
        if (supportTicket.Status != SupportTicketStatus.Open)
            throw new BadRequestException($"SupportTicket with id: {supportTicket.Id} not open");
    }

    private static void ThrowIfSupportTicketHaveSuggestedSolution(SupportTicket supportTicket)
    {
        if (supportTicket.Solutions.Any(_ => _.Status == SolutionStatus.Suggested))
            throw new BadRequestException($"SupportTicket with id: {supportTicket.Id} have suggested solution");
    }

    private static bool IsAccountRelatedToSupportTicket(SupportTicket supportTicket, Guid accountId)
    {
        return supportTicket.User.Id == accountId || supportTicket.Agent?.Id == accountId;
    }
}