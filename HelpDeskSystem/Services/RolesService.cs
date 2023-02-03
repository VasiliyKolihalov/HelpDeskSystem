using AutoMapper;
using HelpDeskSystem.Models.Roles;
using HelpDeskSystem.Repositories;

namespace HelpDeskSystem.Services;

public class RolesService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public RolesService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoleView>> GetAllAsync()
    {
        IEnumerable<Role> roles = await _applicationContext.Roles.GetAllAsync();
        return _mapper.Map<IEnumerable<RoleView>>(roles);
    }

    public async Task<RoleView> GetByIdAsync(Guid id)
    {
        Role role = await _applicationContext.Roles.GetByIdAsync(id);
        return _mapper.Map<RoleView>(role);
    }
}