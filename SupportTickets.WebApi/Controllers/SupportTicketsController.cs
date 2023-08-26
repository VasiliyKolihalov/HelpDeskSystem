using Authentication.Infrastructure.Constants;
using Authentication.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.WebApi.Constants;
using SupportTickets.WebApi.Models.Messages;
using SupportTickets.WebApi.Models.Solutions;
using SupportTickets.WebApi.Models.SupportTicketAgentRecords;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Models.SupportTicketsPages;
using SupportTickets.WebApi.Models.SupportTicketStatusRecords;
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

    [HttpGet("page")]
    [Authorize(Policy = SupportTicketPermissions.GetAll)]
    public async Task<SupportTicketPageView> GetAllPageAsync([FromQuery] SupportTicketPageGet pageGet)
    {
        return await _supportTicketsService.GetAllPageAsync(pageGet);
    }

    [HttpGet("free")]
    [Authorize(Policy = SupportTicketPermissions.GetFree)]
    public async Task<IEnumerable<SupportTicketPreview>> GetFreeAsync()
    {
        return await _supportTicketsService.GetFreeAsync(this.GetAccountIdFromJwt<Guid>());
    }

    [HttpGet("free/page")]
    [Authorize(Policy = SupportTicketPermissions.GetFree)]
    public async Task<SupportTicketPageView> GetFreePageAsync([FromQuery] SupportTicketPageGetFree pageGetFree)
    {
        return await _supportTicketsService.GetFreePageAsync(pageGetFree, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpGet("my")]
    public async Task<IEnumerable<SupportTicketPreview>> GetByAccountAsync()
    {
        return await _supportTicketsService.GetByAccountIdAsync(this.GetAccountIdFromJwt<Guid>());
    }

    [HttpGet("my/page")]
    public async Task<SupportTicketPageView> GetByAccountPageAsync([FromQuery] SupportTicketPageGet pageGet)
    {
        return await _supportTicketsService.GetByAccountIdPageAsync(pageGet, this.GetAccountIdFromJwt<Guid>());
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

    [HttpGet("{id:guid}/agents/history")]
    [Authorize(Policy = SupportTicketPermissions.GetAgentHistory)]
    public async Task<IEnumerable<SupportTicketAgentRecordView>> GetAgentHistoryAsync(Guid id)
    {
        return await _supportTicketsService.GetAgentHistoryAsync(id);
    }

    [HttpPost("{id:guid}/agents/appoint")]
    [Authorize(Roles = RoleNames.Agent)]
    public async Task AppointAgentAsync(Guid id)
    {
        await _supportTicketsService.AppointAgentAsync(id, this.GetAccountFromJwt<Guid>());
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

    #region Status

    [HttpPost("{id:guid}/status/history")]
    [Authorize(Policy = SupportTicketPermissions.GetStatusHistory)]
    public async Task<IEnumerable<SupportTicketStatusRecordView>> GetStatusHistoryAsync(Guid id)
    {
        return await _supportTicketsService.GetStatusHistoryAsync(id);
    }

    [HttpPost("status/solution/suggest")]
    public async Task SuggestSolution(SolutionSuggest solutionSuggest)
    {
        await _supportTicketsService.SuggestSolutionAsync(solutionSuggest, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/status/solutions/accept")]
    public async Task AcceptSolutionAsync(Guid id)
    {
        await _supportTicketsService.AcceptSolutionAsync(id, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/status/solutions/reject")]
    public async Task RejectSolutionAsync(Guid id)
    {
        await _supportTicketsService.RejectSolutionAsync(id, this.GetAccountIdFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/status/close")]
    public async Task CloseAsync(Guid id)
    {
        await _supportTicketsService.CloseAsync(id, this.GetAccountFromJwt<Guid>());
    }

    [HttpPost("{id:guid}/status/reopen")]
    public async Task ReopenAsync(Guid id)
    {
        await _supportTicketsService.ReopenAsync(id, this.GetAccountIdFromJwt<Guid>());
    }

    #endregion
}