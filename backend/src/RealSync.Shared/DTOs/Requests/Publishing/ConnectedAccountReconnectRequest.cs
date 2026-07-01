namespace RealSync.Shared.DTOs.Requests.Publishing;

public record ConnectedAccountReconnectRequest(
    string AccessToken,
    string? RefreshToken,
    int? ExpiresInSeconds
);
