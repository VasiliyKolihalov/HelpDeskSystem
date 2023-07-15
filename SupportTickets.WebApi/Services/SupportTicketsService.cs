using AutoMapper;
using Infrastructure.Exceptions;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.Users;
using SupportTickets.WebApi.Repositories.SupportTickets;
using SupportTickets.WebApi.Repositories.Users;

namespace SupportTickets.WebApi.Services;

public class SupportTicketsService
{
    private readonly ISupportTicketsRepository _supportTicketsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;

    public SupportTicketsService(
        ISupportTicketsRepository supportTicketsRepository,
        IUsersRepository usersRepository,
        IMapper mapper)
    {
        _supportTicketsRepository = supportTicketsRepository;
        _usersRepository = usersRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketsRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<SupportTicketPreview>>(supportTickets);
    }

    public async Task<SupportTicketView> GetByIdAsync(Guid id)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(id)
                                      ?? throw new NotFoundException($"SupportTicket with id: {id} not found");

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketPreview> CreateAsync(SupportTicketCreate supportTicketCreate)
    {
        User user = await _usersRepository.GetByIdAsync(supportTicketCreate.UserId)
                    ?? throw new NotFoundException($"User with id {supportTicketCreate.UserId} not found");

        var supportTicket = _mapper.Map<SupportTicket>(supportTicketCreate);
        supportTicket.Id = Guid.NewGuid();
        supportTicket.User = user;

        await _supportTicketsRepository.InsertAsync(supportTicket);

        return _mapper.Map<SupportTicketPreview>(supportTicket);
    }

    public async Task<SupportTicketPreview> UpdateAsync(SupportTicketUpdate supportTicketUpdate)
    {
        _ = await _supportTicketsRepository.GetByIdAsync(supportTicketUpdate.Id)
            ?? throw new NotFoundException($"SupportTicket with id: {supportTicketUpdate.Id} not found");

        var supportTicket = _mapper.Map<SupportTicket>(supportTicketUpdate);

        await _supportTicketsRepository.UpdateAsync(supportTicket);

        return _mapper.Map<SupportTicketPreview>(supportTicket);
    }

    public async Task<SupportTicketPreview> DeleteAsync(Guid id)
    {
        SupportTicket supportTicket = await _supportTicketsRepository.GetByIdAsync(id)
                                      ?? throw new NotFoundException($"SupportTicket with id: {id} not found");

        await _supportTicketsRepository.DeleteAsync(id);

        return _mapper.Map<SupportTicketPreview>(supportTicket);
    }
}