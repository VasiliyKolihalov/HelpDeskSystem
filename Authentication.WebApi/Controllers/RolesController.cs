using Authentication.WebApi.Models;
using Authentication.WebApi.Models.Roles;
using Authentication.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Authentication.WebApi.Constants.PermissionsConstants;

namespace Authentication.WebApi.Controllers;

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
    [Authorize(Policy = RolesPermissions.Create)]
    public async Task PostAsync(UserRoleCreate userCreate)
    {
        await _rolesService.CreateAsync(userCreate);
    }

    [HttpPut]
    [Authorize(Policy = RolesPermissions.Update)]
    public async Task PutAsync(UserRoleUpdate userUpdate)
    {
        await _rolesService.UpdateAsync(userUpdate);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = RolesPermissions.Delete)]
    public async Task DeleteAsync(string id)
    {
        await _rolesService.DeleteAsync(id);
    }
}