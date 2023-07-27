﻿using Authentication.Infrastructure.Extensions;
using Authentication.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Services;
using static SupportTickets.WebApi.Constants.PermissionsConstants;

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
    [Authorize(Policy = SupportTicketsPermissions.GetAll)]
    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        return await _supportTicketsService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<SupportTicketView> GetAsync(Guid id)
    {
        return await _supportTicketsService.GetByIdAsync(id, this.GetAccountFromJwt());
    }

    [HttpPost]
    public async Task<Guid> PostAsync(SupportTicketCreate supportTicketCreate)
    {
        return await _supportTicketsService.CreateAsync(supportTicketCreate, this.GetAccountFromJwt());
    }

    [HttpPut]
    public async Task PutAsync(SupportTicketUpdate supportTicketUpdate)
    {
        await _supportTicketsService.UpdateAsync(supportTicketUpdate, this.GetAccountFromJwt());
    }

    [HttpDelete]
    public async Task DeleteAsync(Guid id)
    {
        await _supportTicketsService.DeleteAsync(id, this.GetAccountFromJwt());
    }
}