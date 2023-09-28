using Authentication.Api.Models.Roles;
using Infrastructure.Authentication.Models;
using AutoMapper;

namespace Authentication.Api.Profiles;

public class RolesProfile : Profile
{
    public RolesProfile()
    {
        CreateMap<UserRole, UserRoleView>();
        CreateMap<UserRoleCreate, UserRole>();
        CreateMap<UserRole, Role>();
    }
}