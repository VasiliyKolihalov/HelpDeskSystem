using Microsoft.AspNetCore.Mvc;
using Users.WebApi.Models.Users;
using Users.WebApi.Services;

namespace Users.WebApi.Controllers;

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
    public async Task<IEnumerable<UserView>> GetAllAsync()
    {
        return await _usersService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<UserView> GetAsync(Guid id)
    {
        return await _usersService.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<UserView> PostAsync(UserCreate userCreate)
    {
        return await _usersService.CreateAsync(userCreate);
    }

    [HttpPut]
    public async Task<UserView> PutAsync(UserUpdate userUpdate)
    {
        return await _usersService.UpdateAsync(userUpdate);
    }

    [HttpDelete]
    public async Task<UserView> DeleteAsync(Guid id)
    {
        return await _usersService.DeleteAsync(id);
    }
}