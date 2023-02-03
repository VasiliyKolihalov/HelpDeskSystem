using AutoMapper;
using HelpDeskSystem.Models.SupportTicket;
using HelpDeskSystem.Repositories;

namespace HelpDeskSystem.Services;

public class SupportTicketsService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public SupportTicketsService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupportTicketView>> GetAllAsync()
    {
        IEnumerable<SupportTicket> supportTickets = await _applicationContext.SupportTickets.GetAllAsync();
        
        return _mapper.Map<IEnumerable<SupportTicketView>>(supportTickets);
    }

    public async Task<SupportTicketView> GetByIdAsync(Guid id)
    {
        SupportTicket supportTicket = await _applicationContext.SupportTickets.GetByIdAsync(id);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketView> CreateAsync(SupportTicketCreate supportTicketCreate)
    {
        var supportTicket = _mapper.Map<SupportTicket>(supportTicketCreate);
        supportTicket.Id = Guid.NewGuid();
        supportTicket.DateTime = DateTime.Now;

        await _applicationContext.SupportTickets.InsertAsync(supportTicket);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketView> UpdateAsync(SupportTicketUpdate supportTicketUpdate)
    {
        await _applicationContext.SupportTickets.GetByIdAsync(supportTicketUpdate.Id);

        var supportTicket = _mapper.Map<SupportTicket>(supportTicketUpdate);

        await _applicationContext.SupportTickets.UpdateAsync(supportTicket);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }

    public async Task<SupportTicketView> DeleteAsync(Guid id)
    {
        SupportTicket supportTicket = await _applicationContext.SupportTickets.GetByIdAsync(id);

        await _applicationContext.SupportTickets.DeleteAsync(id);

        return _mapper.Map<SupportTicketView>(supportTicket);
    }
}