using System;
using RealSync.Core.Enums;

namespace RealSync.Shared.DTOs.Responses.Publishing;

/// <summary>
/// Thông tin chẩn đoán chi tiết cho một tài khoản liên kết.
/// </summary>
public record ConnectedAccountDiagnosticsResponse(
    Guid AccountId,
    string Provider,
    CredentialStatus Status,
    string TokenStatus,
    TimeSpan? RemainingTtl,
    DateTime? TokenExpiresAt,
    DateTime? LastRefreshAt,
    string? LastRefreshError,
    DateTime? LastValidatedAt,
    string? OaName,
    string? OaDescription,
    long? FollowerCount,
    string? GrantedScopes,
    string? DiagnosticNotes
);
