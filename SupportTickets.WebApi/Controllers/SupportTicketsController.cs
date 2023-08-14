using Authentication.Infrastructure.Constants;
using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.WebApi.Models.Messages;
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

    [HttpGet("my")]
    public async Task<IEnumerable<SupportTicketPreview>> GetBasedOnAccountAsync()
    {
        return await _supportTicketsService.GetBasedOnAccountIdAsync(this.GetAccountIdFromJwt<Guid>());
    }

    [HttpGet("{id:guid}")]
    public async Task<SupportTicketView> GetAsync(Guid id)
    {
        return await _supportTicketsService.GetByIdAsync(id, this.GetAccountFromJwt<Guid>());
    }

    [HttpPost]
    [Authorize(Policy = AccountPolicyNames.EmailConfirm)]
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
    [Authorize(Policy = SupportTicketPermissions.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _supportTicketsService.DeleteAsync(id);
    }


    [HttpPost("{supportTicketId:guid}/close")]
    public async Task CloseAsync(Guid supportTicketId)
    {
        await _supportTicketsService.CloseAsync(supportTicketId, this.GetAccountFromJwt<Guid>());
    }


    #region Agents

    [HttpPost("{supportTicketId:guid}/agent/{userId:guid}/set")]
    [Authorize(Policy = SupportTicketPermissions.SetAgent)]
    public async Task SetAgentAsync(Guid supportTicketId, Guid userId)
    {
        await _supportTicketsService.SetAgentAsync(supportTicketId, userId);
    }

    #endregion

    #region Messages

    [HttpPost("messages")]
    public async Task<Guid> AddMessageAsync(MessageCreate messageCreate)
    {
        return await _supportTicketsService.AddMessageAsync(messageCreate, this.GetAccountFromJwt<Guid>());
    }

    [HttpPut("messages")]
    public async Task UpdateMessageAsync(MessageUpdate messageUpdate)
    {
        await _supportTicketsService.UpdateMessageAsync(messageUpdate, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpDelete("messages/{messageId:guid}")]
    public async Task DeleteMessageAsync(Guid messageId)
    {
        await _supportTicketsService.DeleteMessageAsync(messageId, this.GetAccountIdFromJwt<Guid>());
    }

    #endregion
}