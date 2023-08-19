using Authentication.Infrastructure.Constants;
using Authentication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.WebApi.Constants;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
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

    [HttpGet("free")]
    [Authorize(Policy = SupportTicketPermissions.GetFree)]
    public async Task<IEnumerable<SupportTicketPreview>> GetFreeAsync()
    {
        return await _supportTicketsService.GetFreeAsync(this.GetAccountIdFromJwt<Guid>());
    }

    [HttpGet("my")]
    public async Task<IEnumerable<SupportTicketPreview>> GetByAccountAsync()
    {
        return await _supportTicketsService.GetByAccountIdAsync(this.GetAccountIdFromJwt<Guid>());
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

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = SupportTicketPermissions.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _supportTicketsService.DeleteAsync(id);
    }

    #region Agents

    [HttpPost("{supportTicketId:guid}/agent/appoint")]
    [Authorize(Roles = RoleNames.Agent)]
    public async Task AppointAgentAsync(Guid supportTicketId)
    {
        await _supportTicketsService.AppointAgentAsync(supportTicketId, this.GetAccountFromJwt<Guid>());
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

    #region Solutions

    [HttpPost("solutions/suggest")]
    public async Task SuggestSolution(SolutionSuggest solutionSuggest)
    {
        await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/solutions/accept")]
    public async Task AcceptSolutionAsync(Guid id)
    {
        await _supportTicketsService.AcceptSolutionAsync(id, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/solutions/reject")]
    public async Task RejectSolutionAsync(Guid id)
    {
        await _supportTicketsService.RejectSolutionAsync(id, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/solutions/close")]
    public async Task CloseAsync(Guid id)
    {
        await _supportTicketsService.CloseAsync(id, this.GetAccountFromJwt<Guid>());
    }

    #endregion
}