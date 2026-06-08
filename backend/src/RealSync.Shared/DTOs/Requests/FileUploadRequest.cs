namespace RealSync.Shared.DTOs.Requests;

public class FileUploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
    public Func<Stream> OpenReadStream { get; set; } = null!;
}
