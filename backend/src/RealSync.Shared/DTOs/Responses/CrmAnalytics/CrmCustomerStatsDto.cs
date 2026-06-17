namespace RealSync.Shared.DTOs.Responses.CrmAnalytics;

public class CrmCustomerStatsDto
{
    public int TotalCustomers { get; set; }
    public int CustomersFromLeads { get; set; }
    public int DirectCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public decimal CustomersFromLeadsRate { get; set; }
    public IReadOnlyList<CrmCountByLabelDto> CustomersBySource { get; set; } = Array.Empty<CrmCountByLabelDto>();
}
