using AutoMapper;
using HelpDeskSystem.Models.SupportTicket;

namespace HelpDeskSystem.Profiles;

public class SupportTicketsProfile : Profile
{
    public SupportTicketsProfile()
    {
        CreateMap<SupportTicket, SupportTicketView>();
        CreateMap<SupportTicketUpdate, SupportTicket>();
        CreateMap<SupportTicketCreate, SupportTicket>();
    }
}