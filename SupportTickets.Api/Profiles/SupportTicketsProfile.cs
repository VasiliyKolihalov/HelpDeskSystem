using AutoMapper;
using SupportTickets.Api.Models.SupportTickets;

namespace SupportTickets.Api.Profiles;

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