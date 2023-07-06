using Microsoft.AspNetCore.Mvc;
using SupportTicketWebApi.Models;
using SupportTicketWebApi.Services;

namespace SupportTicketWebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class SupportTicketController : ControllerBase
{
    private readonly SupportTicketService _supportTicketService;

    public SupportTicketController(SupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    [HttpGet]
    public async Task<IEnumerable<SupportTicketView>> GetAllAsync()
    {
        return await _supportTicketService.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<SupportTicketView> GetAsync(Guid id)
    {
        return await _supportTicketService.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<SupportTicketView> PostAsync(SupportTicketCreate supportTicketCreate)
    {
        return await _supportTicketService.CreateAsync(supportTicketCreate);
    }

    [HttpPut]
    public async Task<SupportTicketView> PutAsync(SupportTicketUpdate supportTicketUpdate)
    {
        return await _supportTicketService.UpdateAsync(supportTicketUpdate);
    }

    [HttpDelete("{id:guid}")]
    public async Task<SupportTicketView> DeleteAsync(Guid id)
    {
        return await _supportTicketService.DeleteAsync(id);
    }
}