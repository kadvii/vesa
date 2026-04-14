using System.ComponentModel.DataAnnotations;

namespace eVisaPlatform.Application.DTOs.Setting;

/// <summary>API response shape for a system setting entry.</summary>
public class SettingResponseDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>Input for updating a single system setting's value.</summary>
public class UpdateSettingDto
{
    [Required, MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool? IsPublic { get; set; }
}

/// <summary>Input for creating a new system setting with an explicit key.</summary>
public class CreateSettingDto
{
    /// <summary>
    /// Namespaced key — use "country:{iso}" for countries, "price:{iso}" for prices.
    /// e.g. "country:TR", "price:AE"
    /// </summary>
    [Required, MaxLength(200)]
    public string Key { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;
}
