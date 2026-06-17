namespace RealSync.Shared.DTOs.Responses.CrmAnalytics;

public class CrmAnalyticsSummaryDto
{
    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int ContactedLeads { get; set; }
    public int QualifiedLeads { get; set; }
    public int ProposalLeads { get; set; }
    public int WonLeads { get; set; }
    public int LostLeads { get; set; }
    public int HotLeads { get; set; }
    public int WarmLeads { get; set; }
    public int ColdLeads { get; set; }
    public int TotalCustomers { get; set; }
    public int CustomersFromLeads { get; set; }
    public int DirectCustomers { get; set; }
    public int TotalLeadActivities { get; set; }
    public int OverdueFollowUps { get; set; }
    public int DueTodayFollowUps { get; set; }
    public int UpcomingFollowUps { get; set; }
    public decimal LeadToWonConversionRate { get; set; }
    public decimal LeadToCustomerConversionRate { get; set; }
    public DateTime GeneratedAt { get; set; }
}
