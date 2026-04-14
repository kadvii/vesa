using AutoMapper;
using eVisaPlatform.Application.DTOs.Agents;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;

namespace eVisaPlatform.Application.Services;

public class VisaAgentService : IVisaAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VisaAgentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<VisaAgentResponseDto> AddAgentAsync(CreateVisaAgentDto dto)
    {
        var agent = new VisaAgent
        {
            Id               = Guid.NewGuid(),
            Name             = dto.Name,
            Company          = dto.Company,
            PerformanceScore = 100   // Default starting score
        };

        await _unitOfWork.VisaAgents.AddAsync(agent);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<VisaAgentResponseDto>(agent);
    }

    public async Task AssignAgentAsync(Guid agentId, Guid applicationId)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(applicationId)
                          ?? throw new KeyNotFoundException("Application not found.");

        _ = await _unitOfWork.VisaAgents.GetByIdAsync(agentId)
            ?? throw new KeyNotFoundException("Visa agent not found.");

        application.VisaAgentId = agentId;
        _unitOfWork.VisaApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── User ──────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<VisaAgentResponseDto>> GetAgentsAsync()
    {
        var agents = await _unitOfWork.VisaAgents.GetAllAsync();
        return _mapper.Map<IEnumerable<VisaAgentResponseDto>>(agents);
    }

    public async Task<VisaAgentResponseDto?> GetAgentByIdAsync(Guid id)
    {
        var agent = await _unitOfWork.VisaAgents.GetByIdAsync(id);
        return agent is null ? null : _mapper.Map<VisaAgentResponseDto>(agent);
    }

    /// <summary>
    /// Places an order: assigns this agent to the user's visa application.
    /// </summary>
    public async Task<OrderAgentResponseDto> PlaceOrderAsync(
        Guid agentId, Guid userId, OrderAgentDto dto)
    {
        var application = await _unitOfWork.VisaApplications.GetByIdAsync(dto.ApplicationId)
                          ?? throw new KeyNotFoundException("Visa application not found.");

        if (application.UserId != userId)
            throw new UnauthorizedAccessException("You are not the owner of this application.");

        _ = await _unitOfWork.VisaAgents.GetByIdAsync(agentId)
            ?? throw new KeyNotFoundException("Agent not found.");

        application.VisaAgentId = agentId;
        _unitOfWork.VisaApplications.Update(application);
        await _unitOfWork.SaveChangesAsync();

        return new OrderAgentResponseDto
        {
            Message       = "Order placed with agent successfully.",
            AgentId       = agentId,
            ApplicationId = dto.ApplicationId
        };
    }
}
