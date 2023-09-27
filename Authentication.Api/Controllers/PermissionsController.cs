using Authentication.Api.Models.Permissions;
using Authentication.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly PermissionsService _permissionsService;

    public PermissionsController(PermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    [HttpGet]
    public async Task<IEnumerable<UserPermissionView>> GetAsync()
    {
        return await _permissionsService.GetAllAsync();
    }
}