using eVisaPlatform.Application.DTOs.Agents;

namespace eVisaPlatform.Application.Interfaces;

public interface IVisaAgentService
{
    // Admin operations
    Task<VisaAgentResponseDto> AddAgentAsync(CreateVisaAgentDto dto);
    Task AssignAgentAsync(Guid agentId, Guid applicationId);

    // User operations
    Task<IEnumerable<VisaAgentResponseDto>> GetAgentsAsync();
    Task<VisaAgentResponseDto?> GetAgentByIdAsync(Guid id);
    Task<OrderAgentResponseDto> PlaceOrderAsync(Guid agentId, Guid userId, OrderAgentDto dto);
}
