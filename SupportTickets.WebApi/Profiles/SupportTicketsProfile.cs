using AutoMapper;
using SupportTickets.WebApi.Models.SupportTickets;

namespace SupportTickets.WebApi.Profiles;

public class SupportTicketsProfile : Profile
{
    public SupportTicketsProfile()
    {
        CreateMap<SupportTicket, SupportTicketPreview>();
        CreateMap<SupportTicket, SupportTicketView>();
        CreateMap<SupportTicketUpdate, SupportTicket>();
        CreateMap<SupportTicketCreate, SupportTicket>();
    }
}