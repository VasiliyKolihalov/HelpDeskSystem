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

    [HttpPost("email/sendConfirmCode")]
    [Authorize]
    public async Task SendEmailConfirmCodeAsync()
    {
        await _accountsService.SendEmailConfirmCodeAsync(this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPut("email/confirm/{confirmCode}")]
    [Authorize]
    public async Task ConfirmEmailAsync(string confirmCode)
    {
        await _accountsService.ConfirmEmailAsync(confirmCode, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPut("password/change")]
    [Authorize]
    public async Task ChangePasswordAsync(ChangePassword changePassword)
    {
        await _accountsService.ChangePasswordAsync(changePassword, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPut("email/change")]
    [Authorize]
    public async Task ChangeEmailAsync(ChangeEmail changeEmail)
    {
        await _accountsService.ChangeEmailAsync(changeEmail, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpDelete]
    [Authorize]
    public async Task DeleteAsync()
    {
        await _accountsService.DeleteAsync(this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/roles/{roleId}")]
    [Authorize(Policy = AccountPermissions.AddRole)]
    public async Task AddToRoleAsync(Guid id, string roleId)
    {
        await _accountsService.AddToRoleAsync(id, roleId);
    }

    [HttpDelete("{id:guid}/roles/{roleId}")]
    [Authorize(Policy = AccountPermissions.RemoveRole)]
    public async Task RemoveFromRoleAsync(Guid id, string roleId)
    {
        await _accountsService.RemoveFromRoleAsync(id, roleId);
    }
}