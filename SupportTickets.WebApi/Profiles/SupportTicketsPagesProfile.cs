using AutoMapper;
using SupportTickets.WebApi.Models.SupportTicketsPages;

namespace SupportTickets.WebApi.Profiles;

public class SupportTicketsPagesProfile : Profile
{
    public SupportTicketsPagesProfile()
    {
        CreateMap<SupportTicketPageGet, SupportTicketPageView>();
        CreateMap<SupportTicketPageGetFree, SupportTicketPageView>();
    }
}