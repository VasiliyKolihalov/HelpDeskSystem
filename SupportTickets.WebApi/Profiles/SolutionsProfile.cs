using AutoMapper;
using SupportTickets.WebApi.Models.Solutions;

namespace SupportTickets.WebApi.Profiles;

public class SolutionsProfile : Profile
{
    public SolutionsProfile()
    {
        CreateMap<Solution, SolutionView>();
        CreateMap<SolutionSuggest, Solution>();
    }
}