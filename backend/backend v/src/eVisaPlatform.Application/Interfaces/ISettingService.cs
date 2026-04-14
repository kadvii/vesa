using eVisaPlatform.Application.DTOs.Setting;

namespace eVisaPlatform.Application.Interfaces;

public interface ISettingService
{
    /// <summary>Get all system settings (admins see all, public users see only IsPublic=true).</summary>
    Task<IEnumerable<SettingResponseDto>> GetAllAsync(bool isAdmin = false);

    /// <summary>
    /// Get all settings whose key starts with the given prefix — e.g. "country:" or "price:".
    /// </summary>
    Task<IEnumerable<SettingResponseDto>> GetByPrefixAsync(string prefix, bool isAdmin = false);

    /// <summary>Create a new setting. Throws if the key already exists.</summary>
    Task<SettingResponseDto> CreateAsync(CreateSettingDto dto, string createdByEmail);

    /// <summary>Update a system setting value by key (upsert).</summary>
    Task<SettingResponseDto> UpdateAsync(string key, UpdateSettingDto dto, string updatedByEmail);

    /// <summary>Delete a setting by its ID. Throws KeyNotFoundException if not found.</summary>
    Task DeleteAsync(Guid id);
}
