using eVisaPlatform.Application.DTOs.Agents;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Visa Agents Marketplace: public listing for users, management for Admin/Employee.
/// Route: /api/agents
/// </summary>
[Route("api/agents")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class AgentsController : ControllerBase
{
    private readonly IVisaAgentService _agentService;

    public AgentsController(IVisaAgentService agentService)
        => _agentService = agentService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? Guid.Empty.ToString());

    // ── User-accessible ───────────────────────────────────────────────────────

    /// <summary>List all visa agents [Any authenticated user]</summary>
    [OutputCache(Duration = 120)]
    [HttpGet]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await _agentService.GetAgentsAsync();
        return Ok(agents);
    }

    /// <summary>Get a single agent by ID [Any authenticated user]</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var agent = await _agentService.GetAgentByIdAsync(id);
        return agent is null
            ? NotFound(new { message = "Agent not found." })
            : Ok(agent);
    }

    /// <summary>Place an order with an agent for a visa application [Any authenticated user]</summary>
    [HttpPost("{id:guid}/order")]
    public async Task<IActionResult> PlaceOrder(Guid id, [FromBody] OrderAgentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _agentService.PlaceOrderAsync(id, CurrentUserId, dto);
        return Ok(result);
    }

    // ── Admin / Employee only ─────────────────────────────────────────────────

    /// <summary>Add a new agent [Admin/Employee only]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AddAgent([FromBody] CreateVisaAgentDto dto)
    {
        var agent = await _agentService.AddAgentAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = agent.Id }, agent);
    }

    /// <summary>Assign an agent to a visa application [Admin/Employee only]</summary>
    [HttpPut("{id:guid}/assign/{applicationId:guid}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> AssignAgent(Guid id, Guid applicationId)
    {
        await _agentService.AssignAgentAsync(id, applicationId);
        return NoContent();
    }
}
