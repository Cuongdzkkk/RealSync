using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RealSync.Core.Entities;

namespace RealSync.Services.Publishing;

internal static class YouTubeConnectorSupport
{
    public static YouTubeMediaManifest? ParseManifest(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<YouTubeMediaManifest>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static string BuildVideoResourceJson(ContentVariant variant, YouTubeMediaManifest manifest)
    {
        var resource = new
        {
            snippet = new
            {
                title = manifest.Title ?? variant.Title,
                description = manifest.Description ?? variant.Caption ?? string.Empty,
                tags = manifest.Tags?.Count > 0 ? manifest.Tags : ParseTags(variant.HashtagsJson),
                categoryId = manifest.CategoryId ?? "22",
                defaultLanguage = manifest.DefaultLanguage
            },
            status = new
            {
                privacyStatus = manifest.PrivacyStatus ?? "private",
                selfDeclaredMadeForKids = manifest.SelfDeclaredMadeForKids ?? false,
                containsSyntheticMedia = manifest.ContainsSyntheticMedia ?? false,
                publishAt = manifest.PublishAt
            }
        };

        return JsonSerializer.Serialize(resource, JsonOptions);
    }

    public static string? ExtractString(string json, string propertyName)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }
        catch
        {
            return null;
        }
    }

    public static List<string> ParseScopes(string? scopesJson)
    {
        if (string.IsNullOrWhiteSpace(scopesJson)) return new List<string>();
        if (scopesJson.TrimStart().StartsWith('['))
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(scopesJson, JsonOptions) ?? new List<string>();
            }
            catch { }
        }

        return scopesJson.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public static bool HasScope(List<string> scopes, string scope)
        => scopes.Exists(s => s.Equals(scope, StringComparison.OrdinalIgnoreCase));

    private static List<string>? ParseTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<List<string>>(tagsJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

internal sealed class YouTubeMediaManifest
{
    public string? VideoUrl { get; set; }
    public string? ContentType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
    public string? CategoryId { get; set; }
    public string? PrivacyStatus { get; set; }
    public string? DefaultLanguage { get; set; }
    public bool? SelfDeclaredMadeForKids { get; set; }
    public bool? ContainsSyntheticMedia { get; set; }
    public DateTime? PublishAt { get; set; }
}
