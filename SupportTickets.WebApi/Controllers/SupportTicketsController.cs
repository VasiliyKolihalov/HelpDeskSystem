using Authentication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Services;
using static SupportTickets.WebApi.Constants.PermissionNames;

namespace SupportTickets.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SupportTicketsController : ControllerBase
{
    private readonly SupportTicketsService _supportTicketsService;

    public SupportTicketsController(SupportTicketsService supportTicketsService)
    {
        _supportTicketsService = supportTicketsService;
    }

    [HttpGet]
    [Authorize(Policy = SupportTicketPermissions.GetAll)]
    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        return await _supportTicketsService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<SupportTicketView> GetAsync(Guid id)
    {
        return await _supportTicketsService.GetByIdAsync(id, this.GetAccountFromJwt<Guid>());
    }

    [HttpPost]
    public async Task<Guid> PostAsync(SupportTicketCreate supportTicketCreate)
    {
        return await _supportTicketsService.CreateAsync(supportTicketCreate, this.GetAccountFromJwt<Guid>());
    }

    [HttpPut]
    public async Task PutAsync(SupportTicketUpdate supportTicketUpdate)
    {
        await _supportTicketsService.UpdateAsync(supportTicketUpdate, this.GetAccountFromJwt<Guid>());
    }

    [HttpDelete]
    public async Task DeleteAsync(Guid id)
    {
        await _supportTicketsService.DeleteAsync(id, this.GetAccountFromJwt<Guid>());
    }
}