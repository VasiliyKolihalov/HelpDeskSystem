using Authentication.Api.Models.Permissions;
using Infrastructure.Authentication.Models;
using AutoMapper;

namespace Authentication.Api.Profiles;

public class PermissionsProfile : Profile
{
    public PermissionsProfile()
    {
        CreateMap<UserPermission, UserPermissionView>();
        CreateMap<UserPermission, Permission>();
    }
}