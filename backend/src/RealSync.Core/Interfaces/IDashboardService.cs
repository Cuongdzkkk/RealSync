using RealSync.Shared.DTOs.Responses.Dashboard;

namespace RealSync.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatisticsResponse> GetStatisticsAsync();
    Task<IEnumerable<MonthlyStatItem>> GetMonthlyLeadsAsync();
    Task<IEnumerable<PropertyStatusStatItem>> GetPropertiesByStatusAsync();
}
