using Authentication.Infrastructure.Models;
using AutoMapper;
using Infrastructure.Exceptions;
using SupportTickets.WebApi.Constants;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;
using SupportTickets.WebApi.Services.Clients;
using static SupportTickets.WebApi.Constants.PermissionNames.SupportTicketPermissions;

namespace SupportTickets.WebApi.Services;

public class SupportTicketsService
{
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IAccountsClient _accountsClient;
    private readonly IMapper _mapper;

    public SupportTicketsService(
        ISupportTicketsRepository supportTicketsRepository,
        IUsersRepository usersRepository,
        IAccountsClient accountsClient,
        IMapper mapper)
    {
        _supportTicketsRepository = supportTicketsRepository;
        _usersRepository = usersRepository;
        _accountsClient = accountsClient;
        _mapper = mapper;
    }


    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetBasedOnAccountIdAsync(Guid accountId)
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

    public async Task SetAgentAsync(Guid supportTicketId, Guid userId)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (!await _usersRepository.IsExistsAsync(userId))
            throw new NotFoundException($"User with id: {userId} not found");

        Account<Guid> userAccount = await _accountsClient.SendGetRequestAsync(accountId: userId);

        if (!userAccount.HasRole(RoleNames.Agent))
            throw new BadRequestException("User are not agent");

        supportTicket.Agent = new User { Id = userId };
        await _supportTicketsRepository.UpdateAsync(supportTicket);
    }

    public async Task<Guid> AddMessageAsync(MessageCreate messageCreate, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(messageCreate.SupportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {messageCreate.SupportTicketId} not found");

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

        if (message.User.Id != accountId)
            throw new UnauthorizedException();

        await _supportTicketsRepository.UpdateMessageAsync(_mapper.Map<Message>(messageUpdate));
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid accountId)
    {
        Message message = await _supportTicketsRepository.GetMessageByIdAsync(messageId)
                          ?? throw new NotFoundException($"Message with id: {messageId} not found");

        if (message.User.Id != accountId)
            throw new UnauthorizedException();

        await _supportTicketsRepository.DeleteMessageAsync(messageId);
    }

    private static bool IsAccountRelatedToSupportTicket(SupportTicket supportTicket, Guid accountId)
    {
        return supportTicket.User.Id == accountId || supportTicket.Agent?.Id == accountId;
    }
}