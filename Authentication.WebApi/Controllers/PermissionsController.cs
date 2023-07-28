using Authentication.WebApi.Models.Permissions;
using Authentication.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.WebApi.Controllers;

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