namespace RealSync.Shared.DTOs.Responses.CrmAnalytics;

public class CrmMonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Leads { get; set; }
    public int Customers { get; set; }
    public int ConvertedCustomers { get; set; }
}
