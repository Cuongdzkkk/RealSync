using RealSync.Shared.DTOs.Requests.Crawlers;
using RealSync.Shared.DTOs.Responses.Crawlers;

namespace RealSync.Core.Interfaces;

public interface ICrawlerService
{
    Task<List<CrawlSourceDto>> GetSourcesAsync();
    Task<CrawlSourceDto> CreateSourceAsync(CrawlSourceCreateRequest request);
    Task<CrawlSourceDto> UpdateSourceAsync(Guid id, CrawlSourceUpdateRequest request);
    Task DeleteSourceAsync(Guid id);
    Task<object> RunCrawlerAsync(Guid id, CrawlRunRequest request);
    Task<CrawlStatsDto> GetStatsAsync();
    Task<List<object>> GetJobsAsync();
}
