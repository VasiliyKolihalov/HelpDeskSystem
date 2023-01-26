using HelpDeskSystem.Models;
using HelpDeskSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskSystem.Controllers;

[Route("[controller]")]
[ApiController]
public class SupportTicketsController : ControllerBase
{
    private readonly SupportTicketsService _supportTicketsService;

    public SupportTicketsController(SupportTicketsService supportTicketsService)
    {
        _supportTicketsService = supportTicketsService;
    }

    [HttpGet]
    public async Task<IEnumerable<SupportTicketView>> GetAllAsync()
    {
        return await _supportTicketsService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<SupportTicketView> GetAsync(Guid id)
    {
        return await _supportTicketsService.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<SupportTicketView> PostAsync(SupportTicketCreate supportTicketCreate)
    {
        return await _supportTicketsService.CreateAsync(supportTicketCreate);
    }

    [HttpPut]
    public async Task<SupportTicketView> PutAsync(SupportTicketUpdate supportTicketUpdate)
    {
        return await _supportTicketsService.UpdateAsync(supportTicketUpdate);
    }

    [HttpDelete("{id:guid}")]
    public async Task<SupportTicketView> DeleteAsync(Guid id)
    {
        return await _supportTicketsService.DeleteAsync(id);
    }
}