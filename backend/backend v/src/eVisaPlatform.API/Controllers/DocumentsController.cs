using eVisaPlatform.Application.DTOs.Document;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace eVisaPlatform.API.Controllers;

/// <summary>
/// Document uploads for visa applications.
/// Route: /api/documents
/// </summary>
[ApiController]
[Route("api/documents")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    // ── JWT helpers ───────────────────────────────────────────────────────────
    private Guid CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(raw, out var id) || id == Guid.Empty)
                throw new UnauthorizedAccessException(
                    "Missing or invalid user identifier in the access token.");
            return id;
        }
    }

    // ── UPLOAD ────────────────────────────────────────────────────────────────
    /// <summary>
    /// Upload a document (passport scan, etc.) for a visa application.
    /// Accepts: multipart/form-data
    /// Fields : File (IFormFile), ApplicationId (Guid), FileType (int enum)
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    // Hard server-side limit: reject payloads over 5 MB before ASP.NET reads the body
    [RequestSizeLimit(5 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] UploadRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dto = new UploadDocumentDto
        {
            ApplicationId = req.ApplicationId,
            FileType      = req.FileType,
        };

        var result = await _documentService.UploadAsync(req.File, dto, CurrentUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── LIST for one application ──────────────────────────────────────────────
    /// <summary>Get all documents for a specific visa application.</summary>
    [HttpGet("{applicationId:guid}")]
    public async Task<IActionResult> GetByApplication(Guid applicationId)
    {
        var result = await _documentService.GetByApplicationIdAsync(applicationId);
        return Ok(result);
    }

    // ── GET single ────────────────────────────────────────────────────────────
    /// <summary>Get a single document metadata record by ID.</summary>
    [HttpGet("doc/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _documentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── LIST all (Admin / Employee) ───────────────────────────────────────────
    /// <summary>Get all uploaded documents [Admin or Employee only].</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _documentService.GetAllAsync();
        return Ok(result);
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    /// <summary>Delete a document by ID (deletes both DB record and physical file).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _documentService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ── Request model (inside the controller file for locality) ──────────────────
/// <summary>Multipart form model for the upload endpoint.</summary>
public class UploadRequest
{
    /// <summary>The file to upload (passport scan, photo, or PDF).</summary>
    public required IFormFile File { get; set; }

    /// <summary>ID of the visa application this document belongs to.</summary>
    public Guid ApplicationId { get; set; }

    /// <summary>Document category (Passport = 0, Photo = 1, SupportingDoc = 2, …).</summary>
    public DocumentType FileType { get; set; } = DocumentType.Passport;
}
