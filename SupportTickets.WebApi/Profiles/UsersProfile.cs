using Authentication.Infrastructure.Models;
using AutoMapper;
using SupportTickets.WebApi.Models.Users;

namespace SupportTickets.WebApi.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<User, UserView>();
        CreateMap<Account<Guid>, User>();
    }
}