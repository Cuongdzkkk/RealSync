using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RealSync.Core.Entities;

namespace RealSync.Services.Publishing;

/// <summary>
/// Partial class chua cac phuong thuc lien quan den Zalo OA.
/// </summary>
public partial class ConnectedAccountService
{
    private async Task<(bool IsValid, int ErrorCode, string ErrorMessage, ZaloOaInfo? Info)> CheckZaloTokenValidAsync(
        string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://openapi.zalo.me/v2.0/oa/getoa");
            request.Headers.Add("access_token", accessToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, (int)response.StatusCode, "HTTP " + (int)response.StatusCode, null);
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorProp) && errorProp.GetInt32() != 0)
            {
                var errorCode = errorProp.GetInt32();
                var errorMsg = root.TryGetProperty("message", out var msgProp)
                    ? msgProp.GetString() ?? "Loi khong xac dinh"
                    : "Loi khong xac dinh";
                return (false, errorCode, errorMsg, null);
            }

            ZaloOaInfo? info = null;
            if (root.TryGetProperty("data", out var dataProp))
            {
                info = new ZaloOaInfo(
                    OaId: dataProp.TryGetProperty("oa_id", out var idProp) ? idProp.GetString() : null,
                    OaName: dataProp.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
                    AvatarUrl: dataProp.TryGetProperty("avatar", out var avatarProp) ? avatarProp.GetString() : null,
                    Description: dataProp.TryGetProperty("description", out var descProp) ? descProp.GetString() : null
                );
            }

            return (true, 0, "Success", info);
        }
        catch (Exception ex)
        {
            return (false, -999, "Loi ket noi: " + ex.Message, null);
        }
    }

    private record ZaloOaInfo(string? OaId, string? OaName, string? AvatarUrl, string? Description);
}
