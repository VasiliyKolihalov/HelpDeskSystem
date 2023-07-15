using AutoMapper;
using Users.WebApi.Models.Users;

namespace Users.WebApi.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<User, UserView>();
        CreateMap<UserUpdate, User>();
        CreateMap<UserCreate, User>();
    }
}