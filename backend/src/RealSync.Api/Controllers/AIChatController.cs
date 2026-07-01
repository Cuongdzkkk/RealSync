using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealSync.Core.Interfaces;

namespace RealSync.Api.Controllers;

/// <summary>
/// Controller xử lý hội thoại với AI Copilot.
/// Route: /api/v1/ai-chat
/// </summary>
[Authorize]
[Route("api/v1/ai-chat")]
[ApiController]
public class AIChatController : BaseController
{
    private readonly IAIContentService _aiContentService;

    public AIChatController(IAIContentService aiContentService)
    {
        _aiContentService = aiContentService;
    }

    /// <summary>
    /// Gửi tin nhắn đến AI Copilot.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Shared.DTOs.Responses.ApiResponse<string>), 200)]
    public async Task<IActionResult> Chat([FromBody] AIChatRequest request)
    {
        var response = await _aiContentService.ChatAsync(request.Message);
        return OkResponse(response);
    }
}

public class AIChatRequest
{
    public string Message { get; set; } = string.Empty;
}
