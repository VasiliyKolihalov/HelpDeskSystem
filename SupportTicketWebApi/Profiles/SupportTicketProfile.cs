using AutoMapper;
using SupportTicketWebApi.Models;

namespace SupportTicketWebApi.Profiles;

public class SupportTicketProfile : Profile
{
    public SupportTicketProfile()
    {
        CreateMap<SupportTicket, SupportTicketView>();
        CreateMap<SupportTicketUpdate, SupportTicket>();
        CreateMap<SupportTicketCreate, SupportTicket>();
    }
}