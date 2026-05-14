# 🕷 Crawler Guide — RealSync

> Hướng dẫn xây dựng và vận hành Crawler Engine cho hệ thống RealSync.

---

## 1. Crawler Architecture

```
┌─────────────────────────────────────────────────────┐
│                  Scheduler (Hangfire)                │
│          Chạy theo lịch, trigger crawl jobs          │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│                  Job Manager                         │
│    Quản lý job queue, priority, concurrency          │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│               Crawler Workers                        │
│  ┌──────────────────────────────────────────────┐   │
│  │  Anti-detect Layer                            │   │
│  │  - Proxy rotation                             │   │
│  │  - User-agent rotation                        │   │
│  │  - Request delays (2-5s)                      │   │
│  │  - Fingerprint randomization                  │   │
│  └──────────────────────────────────────────────┘   │
│  ┌─────────┐  ┌─────────┐  ┌──────────────────┐    │
│  │ Fetcher │→ │ Parser  │→ │ Data Transformer │    │
│  └─────────┘  └─────────┘  └──────────────────┘    │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│              Post-processing Pipeline                │
│  ┌──────────┐  ┌───────────┐  ┌─────────────────┐  │
│  │Validator │→ │Deduplicator│→ │  DB Writer      │  │
│  └──────────┘  └───────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────┘
```

---

## 2. Tech Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Browser Automation | Playwright (Node.js) | Render JS, bypass anti-bot |
| HTTP Scraping | Axios + Cheerio | Fast HTML parsing |
| Scheduling | Hangfire (Backend) | Job scheduling |
| Proxy | Rotating proxy service | Anti-detect |
| Storage | SQL Server | Crawl results |

---

## 3. Project Structure

```
crawler/
├── src/
│   ├── index.js                 # Entry point
│   ├── config.js                # Configuration
│   ├── core/
│   │   ├── CrawlerEngine.js     # Main crawler engine
│   │   ├── BrowserManager.js    # Playwright browser pool
│   │   ├── ProxyManager.js      # Proxy rotation
│   │   └── RateLimiter.js       # Request rate limiting
│   │
│   ├── sources/                 # Source-specific crawlers
│   │   ├── BaseCrawler.js       # Abstract base class
│   │   ├── BatDongSanCrawler.js
│   │   ├── ChoTotCrawler.js
│   │   └── NhaDat24hCrawler.js
│   │
│   ├── parsers/                 # Data parsers
│   │   ├── PropertyParser.js
│   │   ├── PriceParser.js
│   │   └── LocationParser.js
│   │
│   ├── validators/              # Data validation
│   │   └── PropertyValidator.js
│   │
│   ├── storage/                 # Data persistence
│   │   ├── DatabaseWriter.js
│   │   └── Deduplicator.js
│   │
│   └── utils/
│       ├── logger.js
│       ├── helpers.js
│       └── constants.js
│
├── configs/
│   ├── sources.json             # Crawl source configurations
│   ├── proxies.json             # Proxy list
│   └── user-agents.json         # User-agent list
│
├── scripts/
│   ├── setup.sh                 # Setup dependencies
│   └── test-crawl.sh            # Test single source
│
├── package.json
└── README.md
```

---

## 4. Source Configuration

### sources.json
```json
{
  "sources": [
    {
      "id": "batdongsan",
      "name": "Batdongsan.com.vn",
      "baseUrl": "https://batdongsan.com.vn",
      "enabled": true,
      "schedule": "0 2 * * *",
      "maxPages": 50,
      "delay": { "min": 3000, "max": 6000 },
      "selectors": {
        "listPage": ".js__card",
        "title": ".js__card-title",
        "price": ".js__card-price",
        "area": ".js__card-area",
        "location": ".js__card-location",
        "detailUrl": ".js__card-title a",
        "nextPage": ".re__pagination-group a:last-child"
      },
      "detailSelectors": {
        "description": ".re__detail-content",
        "specs": ".re__pr-specs-content-item",
        "images": ".re__media-thumb-item img",
        "contactName": ".js__agent-contact-name",
        "contactPhone": ".js__agent-contact-phone"
      }
    }
  ]
}
```

---

## 5. Base Crawler Template

```javascript
// sources/BaseCrawler.js
class BaseCrawler {
  constructor(config, browserManager, proxyManager) {
    this.config = config;
    this.browserManager = browserManager;
    this.proxyManager = proxyManager;
    this.results = [];
    this.errors = [];
    this.stats = { total: 0, success: 0, error: 0, duplicate: 0 };
  }

  async crawl() {
    const browser = await this.browserManager.getBrowser();
    const page = await browser.newPage();

    try {
      await this.setupPage(page);
      
      let currentPage = 1;
      while (currentPage <= this.config.maxPages) {
        const url = this.getPageUrl(currentPage);
        console.log(`[${this.config.name}] Crawling page ${currentPage}: ${url}`);
        
        await page.goto(url, { waitUntil: 'networkidle' });
        
        const items = await this.parseListPage(page);
        
        for (const item of items) {
          try {
            const detail = await this.parseDetailPage(page, item.detailUrl);
            this.results.push({ ...item, ...detail });
            this.stats.success++;
          } catch (err) {
            this.errors.push({ url: item.detailUrl, error: err.message });
            this.stats.error++;
          }
          
          await this.delay();
        }
        
        this.stats.total += items.length;
        
        const hasNext = await this.hasNextPage(page);
        if (!hasNext) break;
        currentPage++;
      }
    } finally {
      await page.close();
    }

    return {
      source: this.config.name,
      results: this.results,
      errors: this.errors,
      stats: this.stats
    };
  }

  // Abstract methods - override in specific crawlers
  getPageUrl(page) { throw new Error('Not implemented'); }
  async parseListPage(page) { throw new Error('Not implemented'); }
  async parseDetailPage(page, url) { throw new Error('Not implemented'); }
  async hasNextPage(page) { throw new Error('Not implemented'); }

  async setupPage(page) {
    // Anti-detect setup
    const proxy = this.proxyManager.getNext();
    const userAgent = this.getRandomUserAgent();
    
    await page.setExtraHTTPHeaders({
      'Accept-Language': 'vi-VN,vi;q=0.9,en;q=0.8'
    });
  }

  async delay() {
    const { min, max } = this.config.delay;
    const ms = Math.floor(Math.random() * (max - min + 1)) + min;
    await new Promise(resolve => setTimeout(resolve, ms));
  }
}

module.exports = BaseCrawler;
```

---

## 6. Crawl Rules

### ⚠ QUAN TRỌNG — Tuân thủ nghiêm ngặt

1. **robots.txt**: Luôn kiểm tra và tuân thủ robots.txt của mỗi nguồn.
2. **Rate Limit**: Tối thiểu 2-5 giây giữa các request.
3. **Concurrency**: Tối đa 2 concurrent crawlers cho cùng domain.
4. **Time Window**: Chỉ crawl từ 00:00 - 06:00 (giảm tải cho nguồn).
5. **Error Tolerance**: Dừng nếu error rate > 30% liên tục 3 pages.
6. **Data Size**: Tối đa 50 pages/source/session.
7. **Proxy Required**: BẮT BUỘC dùng proxy cho production crawl.
8. **Logging**: Log mọi crawl session (source, count, duration, errors).
9. **Deduplication**: Check URL + content hash trước khi lưu.
10. **Validation**: Validate data (required fields, format) trước khi lưu DB.

### Data Quality Rules
- Title: Bắt buộc, ≤ 500 ký tự
- Price: Phải > 0 hoặc "Thương lượng"
- Area: Phải > 0 m²
- Location: Phải có ít nhất District + Province
- Images: Download và lưu local, không hotlink

---

## 7. Error Handling

```javascript
// Retry logic
async function withRetry(fn, maxRetries = 3, baseDelay = 2000) {
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      if (attempt === maxRetries) throw error;
      
      const delay = baseDelay * Math.pow(2, attempt - 1); // Exponential backoff
      console.warn(`Attempt ${attempt} failed, retrying in ${delay}ms...`);
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }
}

// Usage
const pageContent = await withRetry(() => page.goto(url), 3, 3000);
```

---

## 8. Integration with Backend

### API Endpoints (Backend)
```
POST /api/v1/crawl-jobs/start     → Start crawl job
POST /api/v1/crawl-jobs/{id}/stop → Stop crawl job
POST /api/v1/crawl-results        → Submit crawl results (internal)
```

### Workflow
```
1. Backend tạo CrawlJob (status: Pending)
2. Hangfire trigger Crawler service
3. Crawler chạy, gửi kết quả về Backend qua API
4. Backend validate + dedup + lưu vào Properties
5. Backend cập nhật CrawlJob stats (status: Completed)
6. SignalR notify Frontend về kết quả mới
```

---

> Xem thêm: `architecture-guide.md`, `ai-guide.md`
