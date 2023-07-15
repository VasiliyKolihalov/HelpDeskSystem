using Microsoft.AspNetCore.Mvc;
using SupportTickets.WebApi.Models.SupportTickets;
using SupportTickets.WebApi.Services;

namespace SupportTickets.WebApi.Controllers;

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
    public async Task<IEnumerable<SupportTicketPreview>> GetAllAsync()
    {
        return await _supportTicketsService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<SupportTicketView> GetAsync(Guid id)
    {
        return await _supportTicketsService.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<SupportTicketPreview> PostAsync(SupportTicketCreate supportTicketCreate)
    {
        return await _supportTicketsService.CreateAsync(supportTicketCreate);
    }

    [HttpPut]
    public async Task<SupportTicketPreview> PutAsync(SupportTicketUpdate supportTicketUpdate)
    {
        return await _supportTicketsService.UpdateAsync(supportTicketUpdate);
    }

    [HttpDelete]
    public async Task<SupportTicketPreview> DeleteAsync(Guid id)
    {
        return await _supportTicketsService.DeleteAsync(id);
    }
}