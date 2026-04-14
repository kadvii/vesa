namespace eVisaPlatform.Application.DTOs.Agents;

public class CreateVisaAgentDto
{
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
}

public class VisaAgentResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public int PerformanceScore { get; set; }
}

/// <summary>DTO for a user placing an order with an agent</summary>
public class OrderAgentDto
{
    public Guid ApplicationId { get; set; }
    public string? Notes { get; set; }
}

public class OrderAgentResponseDto
{
    public string Message { get; set; } = string.Empty;
    public Guid AgentId { get; set; }
    public Guid ApplicationId { get; set; }
}
