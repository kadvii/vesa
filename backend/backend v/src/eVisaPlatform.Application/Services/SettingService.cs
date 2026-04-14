using AutoMapper;
using eVisaPlatform.Application.DTOs.Setting;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace eVisaPlatform.Application.Services;

public class SettingService : ISettingService
{
    private const string CachePrefix = "settings:all:";
    private readonly IUnitOfWork  _unitOfWork;
    private readonly IMapper      _mapper;
    private readonly IMemoryCache _cache;
    private readonly IDateTimeProvider _clock;

    public SettingService(
        IUnitOfWork       unitOfWork,
        IMapper           mapper,
        IMemoryCache      cache,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
        _cache      = cache;
        _clock      = clock;
    }

    // ── GET ALL ───────────────────────────────────────────────────────────────
    public async Task<IEnumerable<SettingResponseDto>> GetAllAsync(bool isAdmin = false)
    {
        var cacheKey = CachePrefix + (isAdmin ? "admin" : "public");
        if (_cache.TryGetValue(cacheKey, out List<SettingResponseDto>? cached) && cached is not null)
            return cached;

        var all      = await _unitOfWork.Settings.GetAllAsync();
        var filtered = isAdmin ? all : all.Where(s => s.IsPublic);
        var mapped   = _mapper.Map<List<SettingResponseDto>>(filtered.ToList());
        _cache.Set(cacheKey, mapped, TimeSpan.FromMinutes(5));
        return mapped;
    }

    // ── GET BY PREFIX ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<SettingResponseDto>> GetByPrefixAsync(
        string prefix, bool isAdmin = false)
    {
        var all = await GetAllAsync(isAdmin);
        return all.Where(s => s.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    // ── CREATE ────────────────────────────────────────────────────────────────
    public async Task<SettingResponseDto> CreateAsync(
        CreateSettingDto dto, string createdByEmail)
    {
        if (string.IsNullOrWhiteSpace(dto.Key))
            throw new ArgumentException("Key must not be empty.");

        var existing = await _unitOfWork.Settings.GetByKeyAsync(dto.Key);
        if (existing is not null)
            throw new InvalidOperationException($"A setting with key '{dto.Key}' already exists.");

        var setting = new SystemSetting
        {
            Id          = Guid.NewGuid(),
            Key         = dto.Key.Trim(),
            Value       = dto.Value,
            Description = dto.Description,
            IsPublic    = dto.IsPublic,
            UpdatedAt   = _clock.UtcNow,
            UpdatedBy   = createdByEmail,
        };

        await _unitOfWork.Settings.AddAsync(setting);
        await _unitOfWork.SaveChangesAsync();
        BustCache();

        return _mapper.Map<SettingResponseDto>(setting);
    }

    // ── UPDATE (upsert) ───────────────────────────────────────────────────────
    public async Task<SettingResponseDto> UpdateAsync(
        string key, UpdateSettingDto dto, string updatedByEmail)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key must not be empty.");

        var setting = await _unitOfWork.Settings.GetByKeyAsync(key);

        if (setting is null)
        {
            setting = new SystemSetting { Id = Guid.NewGuid(), Key = key };
            await _unitOfWork.Settings.AddAsync(setting);
        }

        setting.Value     = dto.Value;
        setting.UpdatedAt = _clock.UtcNow;
        setting.UpdatedBy = updatedByEmail;

        if (dto.Description is not null) setting.Description = dto.Description;
        if (dto.IsPublic.HasValue)       setting.IsPublic    = dto.IsPublic.Value;

        _unitOfWork.Settings.Update(setting);
        await _unitOfWork.SaveChangesAsync();
        BustCache();

        return _mapper.Map<SettingResponseDto>(setting);
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public async Task DeleteAsync(Guid id)
    {
        var setting = await _unitOfWork.Settings.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Setting with id '{id}' not found.");

        _unitOfWork.Settings.Delete(setting);
        await _unitOfWork.SaveChangesAsync();
        BustCache();
    }

    // ── Cache helpers ─────────────────────────────────────────────────────────
    private void BustCache()
    {
        _cache.Remove(CachePrefix + "admin");
        _cache.Remove(CachePrefix + "public");
    }
}
