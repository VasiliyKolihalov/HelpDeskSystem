using Authentication.Infrastructure.Models;
using AutoMapper;
using Infrastructure.Exceptions;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Services.JobsManagers;
using static SupportTickets.WebApi.Constants.PermissionNames.SupportTicketPermissions;

namespace SupportTickets.WebApi.Services;

public class SupportTicketsService
{
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly IEscalationsManager _escalationsManager;
    private readonly IMapper _mapper;

    private static readonly TimeSpan TimeToEscalation = TimeSpan.FromDays(7);

    public SupportTicketsService(
        ISupportTicketsRepository supportTicketsRepository,
        IEscalationsManager escalationsManager,
        IMapper mapper)
    {
        _supportTicketsRepository = supportTicketsRepository;
        _escalationsManager = escalationsManager;
        _mapper = mapper;
    }


    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetFreeAsync(Guid accountId)
    {
        IEnumerable<SupportTicket> allFreeSupportTickets = await _supportTicketsRepository.GetAllWithoutAgent();

        var availableSupportTickets = new List<SupportTicket>();
        foreach (SupportTicket supportTicket in allFreeSupportTickets)
        {
            IEnumerable<User> formerAgents = await _supportTicketsRepository.GetAgentsHistoryAsync(supportTicket.Id);
            if (formerAgents.Any(_ => _.Id == accountId))
                continue;
            availableSupportTickets.Add(supportTicket);
        }

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(availableSupportTickets);
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetByAccountIdAsync(Guid accountId)
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetBasedOnAccountAsync(accountId);

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<SupportTicketView> GetByIdAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (!IsAccountRelatedToSupportTicket(supportTicket, account.Id) && !account.HasPermission(GetById))
            throw new UnauthorizedException();

        return _mapper.Map<SupportTicketView>(supportTicket);
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

        return supportTicket.Id;
    }

    public async Task UpdateAsync(
        SupportTicketUpdate supportTicketUpdate,
        Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketUpdate.Id)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketUpdate.Id} not found");

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {supportTicketUpdate.Id} not open");

        if (supportTicket.User.Id != account.Id && !account.HasPermission(Update))
            throw new UnauthorizedException();

        var supportTicketUpdated = _mapper.Map<SupportTicket>(supportTicketUpdate);

        await _supportTicketsRepository.UpdateAsync(supportTicketUpdated);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (!await _supportTicketsRepository.IsExistsAsync(id))
            throw new NotFoundException($"SupportTicket with id: {id} not found");

        await _supportTicketsRepository.DeleteAsync(id);
    }

    public async Task AppointAgentAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {supportTicketId} not open");

        if (supportTicket.Agent != null)
            throw new BadRequestException($"SupportTicket with id: {supportTicketId} already have agent");

        IEnumerable<User> formerAgents = await _supportTicketsRepository.GetAgentsHistoryAsync(supportTicketId);

        if (formerAgents.Any(_ => _.Id == account.Id))
            throw new BadRequestException(
                $"Account with id: {account.Id} was already agent for SupportTicket with id: {supportTicketId}");

        supportTicket.Agent = _mapper.Map<User>(account);
        supportTicket.Priority = SupportTicketPriority.Low;
        await _supportTicketsRepository.UpdateAsync(supportTicket);
        await _supportTicketsRepository.AddToAgentsHistoryAsync(supportTicketId, account.Id);

        _escalationsManager.AssignEscalationFor(supportTicketId, TimeToEscalation);
    }

    public async Task<Guid> AddMessageAsync(MessageCreate messageCreate, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(messageCreate.SupportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {messageCreate.SupportTicketId} not found");

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {messageCreate.SupportTicketId} not open");

        if (!IsAccountRelatedToSupportTicket(supportTicket, account.Id))
            throw new UnauthorizedException();

        var message = _mapper.Map<Message>(messageCreate);
        message.Id = Guid.NewGuid();
        message.User = _mapper.Map<User>(account);
        await _supportTicketsRepository.AddMessageAsync(message);

        return message.Id;
    }

    public async Task UpdateMessageAsync(MessageUpdate messageUpdate, Guid accountId)
    {
        Message message = await _supportTicketsRepository.GetMessageByIdAsync(messageUpdate.Id)
                          ?? throw new NotFoundException($"Message with id: {messageUpdate.Id} not found");

        SupportTicket supportTicket = (await _supportTicketsRepository.GetByIdAsync(message.SupportTicketId))!;

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {supportTicket.Id} not open");

        if (message.User.Id != accountId)
            throw new UnauthorizedException();

        await _supportTicketsRepository.UpdateMessageAsync(_mapper.Map<Message>(messageUpdate));
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid accountId)
    {
        Message message = await _supportTicketsRepository.GetMessageByIdAsync(messageId)
                          ?? throw new NotFoundException($"Message with id: {messageId} not found");

        SupportTicket supportTicket = (await _supportTicketsRepository.GetByIdAsync(message.SupportTicketId))!;

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {supportTicket.Id} not open");

        if (message.User.Id != accountId)
            throw new UnauthorizedException();

        await _supportTicketsRepository.DeleteMessageAsync(messageId);
    }

    public async Task SuggestSolutionAsync(SolutionSuggest solutionSuggest, Guid accountId)
    {
        Message message = await _supportTicketsRepository.GetMessageByIdAsync(solutionSuggest.MessageId)
                          ?? throw new NotFoundException($"Message with id: {solutionSuggest.MessageId} not found");

        SupportTicket supportTicket = (await _supportTicketsRepository.GetByIdAsync(message.SupportTicketId))!;

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {supportTicket.Id} not open");

        if (supportTicket.Agent?.Id != accountId)
            throw new UnauthorizedException();

        if (supportTicket.Solutions.Any(_ => _.Status == SolutionStatus.Suggested))
            throw new BadRequestException("Exists solution which already suggested");

        if (supportTicket.Solutions.Any(_ => _.MessageId == solutionSuggest.MessageId))
            throw new BadRequestException($"Message with id: {solutionSuggest.MessageId} was already solution");

        var solution = _mapper.Map<Solution>(solutionSuggest);
        solution.Status = SolutionStatus.Suggested;
        await _supportTicketsRepository.AddSolutionAsync(solution);
    }

    public async Task AcceptSolutionAsync(Guid supportTicketId, Guid accountId)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (supportTicket.User.Id != accountId)
            throw new UnauthorizedException();

        Solution solution = supportTicket.Solutions.FirstOrDefault(_ => _.Status == SolutionStatus.Suggested)
                            ?? throw new BadRequestException("Suggested solution not exists");

        solution.Status = SolutionStatus.Accepted;
        await _supportTicketsRepository.UpdateSolutionAsync(solution);

        supportTicket.Status = SupportTicketStatus.Solved;
        await _supportTicketsRepository.UpdateAsync(supportTicket);

        _escalationsManager.CancelEscalationFor(supportTicketId);
    }

    public async Task RejectSolutionAsync(Guid supportTicketId, Guid accountId)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (supportTicket.User.Id != accountId)
            throw new UnauthorizedException();

        Solution solution = supportTicket.Solutions.FirstOrDefault(_ => _.Status == SolutionStatus.Suggested)
                            ?? throw new BadRequestException("Suggested solution not exists");

        solution.Status = SolutionStatus.Rejected;
        await _supportTicketsRepository.UpdateSolutionAsync(solution);
    }

    public async Task CloseAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (!IsSupportTicketOpen(supportTicket))
            throw new BadRequestException($"SupportTicket with id: {supportTicketId} not open");

        if (supportTicket.Agent?.Id != account.Id && !account.HasPermission(Close))
            throw new UnauthorizedException();

        supportTicket.Status = SupportTicketStatus.Close;
        await _supportTicketsRepository.UpdateAsync(supportTicket);

        _escalationsManager.CancelEscalationFor(supportTicketId);
    }

    private static bool IsSupportTicketOpen(SupportTicket supportTicket)
    {
        return supportTicket.Status == SupportTicketStatus.Open;
    }

    private static bool IsAccountRelatedToSupportTicket(SupportTicket supportTicket, Guid accountId)
    {
        return supportTicket.User.Id == accountId || supportTicket.Agent?.Id == accountId;
    }
}