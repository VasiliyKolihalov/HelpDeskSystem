using Authentication.Infrastructure.Extensions;
using Authentication.WebApi.Models.Accounts;
using Authentication.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Authentication.WebApi.Constants.PermissionNames;

namespace Authentication.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AccountsService _accountsService;

    public AccountsController(AccountsService accountsService)
    {
        _accountsService = accountsService;
    }

    [HttpPost("register")]
    public async Task<string> RegisterAsync(UserAccountRegister userAccountRegister)
    {
        return await _accountsService.RegisterAsync(userAccountRegister);
    }

    [HttpPost("login")]
    public async Task<string> LoginAsync(UserAccountLogin userAccountLogin)
    {
        return await _accountsService.LoginAsync(userAccountLogin);
    }

    [HttpPut("password/change")]
    [Authorize]
    public async Task ChangePasswordAsync(UserAccountChangePassword userAccountChangePassword)
    {
        await _accountsService.ChangePassword(userAccountChangePassword, this.GetAccountFromJwt<Guid>());
    }

    [HttpDelete]
    [Authorize]
    public async Task DeleteAsync()
    {
        await _accountsService.DeleteAccount(this.GetAccountFromJwt<Guid>());
    }

    [HttpPost("{accountId:guid}/roles/{roleId}")]
    [Authorize(Policy = AccountPermissions.AddRole)]
    public async Task AddToRoleAsync(Guid accountId, string roleId)
    {
        await _accountsService.AddToRoleAsync(accountId, roleId);
    }

    [HttpDelete("{accountId:guid}/roles/{roleId}")]
    [Authorize(Policy = AccountPermissions.RemoveRole)]
    public async Task RemoveFromRoleAsync(Guid accountId, string roleId)
    {
        await _accountsService.RemoveFromRoleAsync(accountId, roleId);
    }
}