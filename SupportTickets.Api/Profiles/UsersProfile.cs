using Infrastructure.Authentication.Models;
using AutoMapper;
using SupportTickets.Api.Models.Users;

namespace SupportTickets.Api.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<User, UserView>();
        CreateMap<Account<Guid>, User>();
    }
}