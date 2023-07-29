using Authentication.Infrastructure.Models;
using Authentication.WebApi.Models.Permissions;
using AutoMapper;

namespace Authentication.WebApi.Profiles;

public class PermissionsProfile : Profile
{
    public PermissionsProfile()
    {
        CreateMap<UserPermission, UserPermissionView>();
        CreateMap<UserPermission, Permission>();
    }
}