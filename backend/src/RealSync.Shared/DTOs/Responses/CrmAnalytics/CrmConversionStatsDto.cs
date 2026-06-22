namespace RealSync.Shared.DTOs.Responses.CrmAnalytics;

public class CrmConversionStatsDto
{
    public int TotalLeads { get; set; }
    public int WonLeads { get; set; }
    public int LostLeads { get; set; }
    public int CustomersFromLeads { get; set; }
    public int DirectCustomers { get; set; }
    public decimal LeadToWonConversionRate { get; set; }
    public decimal LeadToCustomerConversionRate { get; set; }
    public decimal LostRate { get; set; }
}
