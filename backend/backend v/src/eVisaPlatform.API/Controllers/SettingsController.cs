using eVisaPlatform.Application.DTOs.Setting;
using eVisaPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// System settings + scoped CRUD for Countries and Pricing.
///
/// Key namespaces stored in SystemSettings:
///   country:{iso}  →  value = Country display name (Arabic)
///   price:{iso}    →  value = fee in USD (string)
/// </summary>
[ApiController]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly ISettingService _settingService;

    public SettingsController(ISettingService settingService)
    {
        _settingService = settingService;
    }

    private bool   IsAdmin           => User.IsInRole("Admin");
    private string CurrentUserEmail  => User.FindFirstValue(ClaimTypes.Email)
                                        ?? User.FindFirstValue("email")
                                        ?? "system";

    // ══════════════════════════════════════════════════════════════════════════
    //  GENERIC SETTINGS  —  /api/settings
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("api/settings")]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _settingService.GetAllAsync(IsAdmin);
        return Ok(result);
    }

    [HttpPost("api/settings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSettingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _settingService.CreateAsync(dto, CurrentUserEmail);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("api/settings/{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSettingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _settingService.UpdateAsync(key, dto, CurrentUserEmail);
        return Ok(result);
    }

    [HttpDelete("api/settings/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _settingService.DeleteAsync(id);
        return NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  COUNTRIES  —  /api/countries
    //  Each record: key = "country:{iso}"  value = Arabic display name
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>GET /api/countries — list all available destination countries.</summary>
    [HttpGet("api/countries")]
    [Authorize]
    public async Task<IActionResult> GetCountries()
    {
        var items = await _settingService.GetByPrefixAsync("country:", IsAdmin);
        var result = items.Select(MapCountry);
        return Ok(result);
    }

    /// <summary>POST /api/countries — add a new destination country [Admin only].</summary>
    [HttpPost("api/countries")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCountry([FromBody] CountryUpsertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = $"country:{dto.IsoCode.Trim().ToUpperInvariant()}";
        var setting = await _settingService.CreateAsync(new CreateSettingDto
        {
            Key         = key,
            Value       = dto.Name.Trim(),
            Description = dto.Description,
            IsPublic    = dto.IsActive,
        }, CurrentUserEmail);

        return CreatedAtAction(nameof(GetCountries), MapCountry(setting));
    }

    /// <summary>PUT /api/countries/{iso} — update a country's name [Admin only].</summary>
    [HttpPut("api/countries/{iso}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCountry(string iso, [FromBody] CountryUpsertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = $"country:{iso.Trim().ToUpperInvariant()}";
        var setting = await _settingService.UpdateAsync(key, new UpdateSettingDto
        {
            Value       = dto.Name.Trim(),
            Description = dto.Description,
            IsPublic    = dto.IsActive,
        }, CurrentUserEmail);

        return Ok(MapCountry(setting));
    }

    /// <summary>DELETE /api/countries/{id:guid} — remove a country [Admin only].</summary>
    [HttpDelete("api/countries/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCountry(Guid id)
    {
        await _settingService.DeleteAsync(id);
        return NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PRICING  —  /api/prices
    //  Each record: key = "price:{iso}"  value = fee amount in USD
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>GET /api/prices — list all visa fee prices.</summary>
    [HttpGet("api/prices")]
    [Authorize]
    public async Task<IActionResult> GetPrices()
    {
        var items  = await _settingService.GetByPrefixAsync("price:", IsAdmin);
        var result = items.Select(MapPrice);
        return Ok(result);
    }

    /// <summary>POST /api/prices — add a new country price [Admin only].</summary>
    [HttpPost("api/prices")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePrice([FromBody] PriceUpsertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = $"price:{dto.IsoCode.Trim().ToUpperInvariant()}";
        var setting = await _settingService.CreateAsync(new CreateSettingDto
        {
            Key         = key,
            Value       = dto.AmountUsd.ToString("F2"),
            Description = dto.Description,
            IsPublic    = false,   // prices are admin-only by default
        }, CurrentUserEmail);

        return CreatedAtAction(nameof(GetPrices), MapPrice(setting));
    }

    /// <summary>PUT /api/prices/{iso} — update a country's visa fee [Admin only].</summary>
    [HttpPut("api/prices/{iso}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePrice(string iso, [FromBody] PriceUpsertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = $"price:{iso.Trim().ToUpperInvariant()}";
        var setting = await _settingService.UpdateAsync(key, new UpdateSettingDto
        {
            Value       = dto.AmountUsd.ToString("F2"),
            Description = dto.Description,
        }, CurrentUserEmail);

        return Ok(MapPrice(setting));
    }

    /// <summary>DELETE /api/prices/{id:guid} — remove a price entry [Admin only].</summary>
    [HttpDelete("api/prices/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePrice(Guid id)
    {
        await _settingService.DeleteAsync(id);
        return NoContent();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  REQUIRED DOCUMENTS  —  /api/reqdocs
    //  Each record: key = "reqdoc:{slug}"
    // ══════════════════════════════════════════════════════════════════════════

    [HttpGet("api/reqdocs")]
    [Authorize]
    public async Task<IActionResult> GetReqDocs()
    {
        var items  = await _settingService.GetByPrefixAsync("reqdoc:", IsAdmin);
        var result = items.Select(MapReqDoc);
        return Ok(result);
    }

    [HttpPost("api/reqdocs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateReqDoc([FromBody] ReqDocUpsertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = $"reqdoc:{dto.Slug.Trim().ToLowerInvariant()}";
        var setting = await _settingService.CreateAsync(new CreateSettingDto
        {
            Key         = key,
            Value       = dto.Name.Trim(),
            Description = dto.Description,
            IsPublic    = dto.IsMandatory,
        }, CurrentUserEmail);

        return CreatedAtAction(nameof(GetReqDocs), MapReqDoc(setting));
    }

    [HttpPut("api/reqdocs/{slug}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateReqDoc(string slug, [FromBody] ReqDocUpsertDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = $"reqdoc:{slug.Trim().ToLowerInvariant()}";
        var setting = await _settingService.UpdateAsync(key, new UpdateSettingDto
        {
            Value       = dto.Name.Trim(),
            Description = dto.Description,
            IsPublic    = dto.IsMandatory,
        }, CurrentUserEmail);

        return Ok(MapReqDoc(setting));
    }

    [HttpDelete("api/reqdocs/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteReqDoc(Guid id)
    {
        await _settingService.DeleteAsync(id);
        return NoContent();
    }

    // ── Projection helpers ────────────────────────────────────────────────────
    private static CountryResponseDto MapCountry(SettingResponseDto s) => new()
    {
        Id          = s.Id,
        IsoCode     = s.Key.Replace("country:", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant(),
        Name        = s.Value,
        Description = s.Description,
        IsActive    = s.IsPublic,
        UpdatedAt   = s.UpdatedAt,
    };

    private static PriceResponseDto MapPrice(SettingResponseDto s) => new()
    {
        Id          = s.Id,
        IsoCode     = s.Key.Replace("price:", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant(),
        AmountUsd   = decimal.TryParse(s.Value, out var d) ? d : 0,
        Description = s.Description,
        UpdatedAt   = s.UpdatedAt,
    };

    private static ReqDocResponseDto MapReqDoc(SettingResponseDto s) => new()
    {
        Id          = s.Id,
        Slug        = s.Key.Replace("reqdoc:", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant(),
        Name        = s.Value,
        Description = s.Description,
        IsMandatory = s.IsPublic,
        UpdatedAt   = s.UpdatedAt,
    };
}

// ── Inline DTOs (scoped, only needed by this controller) ─────────────────────

public class CountryUpsertDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(10)]
    public string IsoCode { get; set; } = string.Empty;   // e.g. "TR", "AE"

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = string.Empty;      // e.g. "تركيا"

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class PriceUpsertDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(10)]
    public string IsoCode { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(0.01, 100_000)]
    public decimal AmountUsd { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }
}

public class CountryResponseDto
{
    public Guid Id { get; set; }
    public string IsoCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PriceResponseDto
{
    public Guid    Id { get; set; }
    public string  IsoCode { get; set; } = string.Empty;
    public decimal AmountUsd { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReqDocUpsertDto
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string Slug { get; set; } = string.Empty;   // e.g. "passport", "bank-statement"

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = string.Empty;   // e.g. "صورة الجواز"

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Description { get; set; }

    public bool IsMandatory { get; set; } = true;
}

public class ReqDocResponseDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime UpdatedAt { get; set; }
}
