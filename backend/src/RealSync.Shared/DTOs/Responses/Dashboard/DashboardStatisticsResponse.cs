namespace RealSync.Shared.DTOs.Responses.Dashboard;

public class DashboardStatisticsResponse
{
    public int TotalLeads { get; set; }
    public int TotalProperties { get; set; }
    public int TotalCustomers { get; set; }
    
    // Growth rates compared to last month
    public decimal LeadsGrowthRate { get; set; }
    public decimal PropertiesGrowthRate { get; set; }
    public decimal CustomersGrowthRate { get; set; }
}

public class MonthlyStatItem
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PropertyStatusStatItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
