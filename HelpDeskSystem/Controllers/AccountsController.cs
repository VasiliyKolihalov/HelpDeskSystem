using HelpDeskSystem.Models.User;
using HelpDeskSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly AccountsService _accountsService;

    public AccountsController(AccountsService accountsService)
    {
        _accountsService = accountsService;
    }

    [Route("register")]
    [HttpPost]
    public async Task<ActionResult<string>> RegisterAsync(UserCreate userCreate)
    {
        return Ok(new { token = await _accountsService.RegisterAsync(userCreate) });
    }

    [Route("login")]
    [HttpPost]
    public async Task<ActionResult<string>> LoginAsync(UserLogin userLogin)
    {
        return Ok(new { token = await _accountsService.LoginAsync(userLogin) });
    }
}