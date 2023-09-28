using Authentication.Api.Models.Roles;
using Authentication.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Authentication.Api.Constants.PermissionNames;

namespace Authentication.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly RolesService _rolesService;

    public RolesController(RolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet]
    public async Task<IEnumerable<UserRoleView>> GetAllAsync()
    {
        return await _rolesService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<UserRoleView> GetAsync(string id)
    {
        return await _rolesService.GetByIdAsync(id);
    }

    [HttpPost]
    [Authorize(Policy = RolePermissions.Create)]
    public async Task PostAsync(UserRoleCreate userCreate)
    {
        await _rolesService.CreateAsync(userCreate);
    }

    [HttpPut]
    [Authorize(Policy = RolePermissions.Update)]
    public async Task PutAsync(UserRoleUpdate userUpdate)
    {
        await _rolesService.UpdateAsync(userUpdate);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = RolePermissions.Delete)]
    public async Task DeleteAsync(string id)
    {
        await _rolesService.DeleteAsync(id);
    }
}