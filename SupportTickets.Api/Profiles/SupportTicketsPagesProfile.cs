using AutoMapper;
using SupportTickets.Api.Models.SupportTicketsPages;

namespace SupportTickets.Api.Profiles;

public class SupportTicketsPagesProfile : Profile
{
    public SupportTicketsPagesProfile()
    {
        CreateMap<SupportTicketPageGet, SupportTicketPageView>();
        CreateMap<SupportTicketPageGetFree, SupportTicketPageView>();
    }
}