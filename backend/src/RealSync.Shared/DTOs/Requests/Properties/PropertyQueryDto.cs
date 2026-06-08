using RealSync.Shared.DTOs.Requests;

namespace RealSync.Shared.DTOs.Requests.Properties;

public class PropertyQueryDto : PaginationRequest
{
    public string? Keyword { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? WardId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TypeId { get; set; }
    public string? Status { get; set; }
}
