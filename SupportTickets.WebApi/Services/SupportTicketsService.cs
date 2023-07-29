using Authentication.Infrastructure.Models;
using AutoMapper;
using Infrastructure.Exceptions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.SupportTickets;
using static SupportTickets.WebApi.Constants.PermissionNames.SupportTicketPermissions;

namespace SupportTickets.WebApi.Services;

public class SupportTicketsService
{
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly IMapper _mapper;

    public SupportTicketsService(
        ISupportTicketsRepository supportTicketsRepository,
        IMapper mapper)
    {
        _supportTicketsRepository = supportTicketsRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<SupportTicketView> GetByIdAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (supportTicket.User.Id != account.Id && !account.HasPermission(GetById))
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

    public async Task DeleteAsync(Guid supportTicketId, Account<Guid> account)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(supportTicketId)
                                      ?? throw new NotFoundException(
                                          $"SupportTicket with id: {supportTicketId} not found");

        if (supportTicket.User.Id != account.Id && !account.HasPermission(Delete))
            throw new UnauthorizedException();
        
        await _supportTicketsRepository.DeleteAsync(supportTicketId);
    }
}