using RealSync.Shared.DTOs.Requests.CrmAnalytics;
using RealSync.Shared.DTOs.Responses.CrmAnalytics;

namespace RealSync.Core.Interfaces;

public interface ICrmAnalyticsService
{
    Task<CrmAnalyticsSummaryDto> GetSummaryAsync(CrmAnalyticsQueryDto query);
    Task<IReadOnlyList<CrmCountByLabelDto>> GetLeadsByStatusAsync(CrmAnalyticsQueryDto query);
    Task<IReadOnlyList<CrmCountByLabelDto>> GetLeadsBySourceAsync(CrmAnalyticsQueryDto query);
    Task<CrmConversionStatsDto> GetConversionStatsAsync(CrmAnalyticsQueryDto query);
    Task<CrmFollowUpStatsDto> GetFollowUpStatsAsync(CrmAnalyticsQueryDto query);
    Task<CrmCustomerStatsDto> GetCustomerStatsAsync(CrmAnalyticsQueryDto query);
    Task<IReadOnlyList<CrmMonthlyTrendDto>> GetMonthlyTrendAsync(int? year);
}
