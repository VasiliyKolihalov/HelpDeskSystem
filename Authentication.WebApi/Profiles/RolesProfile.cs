using Authentication.Infrastructure.Models;
using Authentication.WebApi.Models.Roles;
using AutoMapper;

namespace Authentication.WebApi.Profiles;

public class RolesProfile : Profile
{
    public RolesProfile()
    {
        CreateMap<UserRole, UserRoleView>();
        CreateMap<UserRoleCreate, UserRole>();
        CreateMap<UserRole, Role>();
    }
}