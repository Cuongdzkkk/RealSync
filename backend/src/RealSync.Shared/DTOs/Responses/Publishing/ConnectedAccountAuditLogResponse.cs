using System;

namespace RealSync.Shared.DTOs.Responses.Publishing;

public record ConnectedAccountAuditLogResponse(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string Action,
    string? Description,
    string? IpAddress,
    DateTime CreatedAt
);
