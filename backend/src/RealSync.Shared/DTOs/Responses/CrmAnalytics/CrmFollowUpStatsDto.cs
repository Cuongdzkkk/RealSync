namespace RealSync.Shared.DTOs.Responses.CrmAnalytics;

public class CrmFollowUpStatsDto
{
    public int TotalLeadsWithFollowUp { get; set; }
    public int OverdueFollowUps { get; set; }
    public int DueTodayFollowUps { get; set; }
    public int UpcomingFollowUps { get; set; }
    public int NoFollowUpLeads { get; set; }
}
