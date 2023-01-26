using AutoMapper;
using HelpDeskSystem.Models;

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