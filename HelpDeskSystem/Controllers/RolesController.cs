using HelpDeskSystem.Models.Roles;
using HelpDeskSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly RolesService _rolesService;

    public RolesController(RolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet]
    public async Task<IEnumerable<RoleView>> GetAllAsync()
    {
        return await _rolesService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<RoleView> GetAsync(Guid id)
    {
        return await _rolesService.GetByIdAsync(id);
    }
}