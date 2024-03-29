﻿using Infrastructure.Authentication.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Api.Models.Users;
using Users.Api.Services;
using static Users.Api.Constants.PermissionNames;

namespace Users.Api.Controllers;

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
    [Authorize(Policy = UserPermissions.Create)]
    public async Task<Guid> PostAsync(UserCreate userCreate)
    {
        return await _usersService.CreateAsync(userCreate);
    }

    [HttpPut]
    [Authorize]
    public async Task PutAsync(UserUpdate userUpdate)
    {
        await _usersService.UpdateAsync(userUpdate, this.GetAccountFromJwt<Guid>());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = UserPermissions.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _usersService.DeleteAsync(id);
    }
}