using AutoMapper;
using HelpDeskSystem.Models.Roles;

namespace HelpDeskSystem.Profiles;

public class RolesProfile : Profile
{
    public RolesProfile()
    {
        CreateMap<Role, RolePreview>();
        CreateMap<Role, RoleView>();
    }
}