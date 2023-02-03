using AutoMapper;
using HelpDeskSystem.Models.User;

namespace HelpDeskSystem.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<User, UserPreview>();
        CreateMap<User, UserView>();
        CreateMap<UserUpdate, User>();
        CreateMap<UserCreate, User>();
    }
}