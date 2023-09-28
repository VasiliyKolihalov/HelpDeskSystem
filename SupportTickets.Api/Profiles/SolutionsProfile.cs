using AutoMapper;
using SupportTickets.Api.Models.Solutions;

namespace SupportTickets.Api.Profiles;

public class SolutionsProfile : Profile
{
    public SolutionsProfile()
    {
        CreateMap<Solution, SolutionView>();
        CreateMap<SolutionSuggest, Solution>();
    }
}