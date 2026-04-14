using eVisaPlatform.Application.DTOs.Support;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketDto dto)
    {
        var ticket = await _supportService.CreateTicketAsync(GetUserId(), dto);
        return Ok(ticket);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTickets()
    {
        var tickets = await _supportService.GetUserTicketsAsync(GetUserId());
        return Ok(tickets);
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> AddReply(Guid id, [FromBody] CreateTicketReplyDto dto)
    {
        var reply = await _supportService.AddReplyAsync(GetUserId(), id, dto);
        return Ok(reply);
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseTicket(Guid id)
    {
        await _supportService.CloseTicketAsync(GetUserId(), id);
        return NoContent();
    }
}
