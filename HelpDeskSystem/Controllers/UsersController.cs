using HelpDeskSystem.Extensions;
using HelpDeskSystem.Models.User;
using HelpDeskSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    public async Task<IEnumerable<UserPreview>> GetAllAsync()
    {
        return await _usersService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<UserView> GetAsync(Guid id)
    {
        return await _usersService.GetByIdAsync(id);
    }

    [HttpPut]
    public async Task<UserPreview> PutAsync(UserUpdate userUpdate)
    {
        return await _usersService.UpdateAsync(userUpdate, requestUserId: this.GetUserId());
    }

    [HttpDelete("{id:guid}")]
    public async Task<UserPreview> DeleteAsync(Guid id)
    {
        return await _usersService.DeleteAsync(id, requestUserId: this.GetUserId());
    }
}