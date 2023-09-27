using AutoMapper;
using Users.Api.Models.Users;

namespace Users.Api.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<User, UserView>();
        CreateMap<UserUpdate, User>();
        CreateMap<UserCreate, User>();
    }
}