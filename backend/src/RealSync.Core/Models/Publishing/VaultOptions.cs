namespace RealSync.Core.Models.Publishing;

public class VaultOptions
{
    public const string SectionName = "Vault";
    public string MasterKey { get; set; } = string.Empty;
    public string ActiveVersion { get; set; } = "v1";
}
