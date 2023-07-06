using AutoMapper;
using Infrastructure.Exceptions;
using SupportTicketWebApi.Models;
using SupportTicketWebApi.Repositories;


namespace SupportTicketWebApi.Services;

public class SupportTicketService
{
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly IMapper _mapper;

    public SupportTicketService(ISupportTicketRepository supportTicketRepository, IMapper mapper)
    {
        _supportTicketRepository = supportTicketRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupportTicketView>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _supportTicketRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<SupportTicketView>>(supportTickets);
    }

    public async Task<SupportTicketView> GetByIdAsync(Guid id)
    {
        SupportTicket supportTicket = await _supportTicketRepository.GetByIdAsync(id)
                                      ?? throw new NotFoundException($"SupportTicket with id: {id} not found");

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketView> CreateAsync(SupportTicketCreate supportTicketCreate)
    {
        var supportTicket = _mapper.Map<SupportTicket>(supportTicketCreate);
        supportTicket.Id = Guid.NewGuid();

        await _supportTicketRepository.InsertAsync(supportTicket);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketView> UpdateAsync(SupportTicketUpdate supportTicketUpdate)
    {
        _ = await _supportTicketRepository.GetByIdAsync(supportTicketUpdate.Id)
            ?? throw new NotFoundException($"SupportTicket with id: {supportTicketUpdate.Id} not found");

        var supportTicket = _mapper.Map<SupportTicket>(supportTicketUpdate);

        await _supportTicketRepository.UpdateAsync(supportTicket);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketView> DeleteAsync(Guid id)
    {
        SupportTicket supportTicket = await _supportTicketRepository.GetByIdAsync(id)
                                      ?? throw new NotFoundException($"SupportTicket with id: {id} not found");

        await _supportTicketRepository.DeleteAsync(id);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }
}