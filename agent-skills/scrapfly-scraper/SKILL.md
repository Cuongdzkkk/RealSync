---
name: scrapfly-scraper
description: Web scraping API với proxy rotation, anti-bot bypass, JavaScript rendering. Dùng khi cần scrape trang web bị chống bot (Cloudflare, DataDome, PerimeterX).
---

# Scrapfly Scraper — Anti-Detect Web Scraping

**Nguồn:** https://github.com/scrapfly/skills

API chuyên nghiệp để scrape web với:
- ✅ Chống bot (Cloudflare, DataDome, PerimeterX, Kasada)
- ✅ Proxy rotation + geo-targeting
- ✅ JavaScript rendering
- ✅ Session persistence
- ✅ Screenshot, markdown export

## Cài đặt

```bash
pip install scrapfly-sdk
export SCRAPFLY_API_KEY="your-api-key"
```

## Sử dụng cơ bản

```python
from scrapfly import ScrapflyClient, ScrapeConfig
import os

client = ScrapflyClient(key=os.environ["SCRAPFLY_API_KEY"])

# Cơ bản
result = client.scrape(ScrapeConfig(url="https://httpbin.dev"))
print(result.content)

# Chống bot + proxy residential
result = client.scrape(ScrapeConfig(
    url="https://target-site.com",
    asp=True,
    country="us",
    proxy_pool="public_residential_pool",
))

# JavaScript rendering
result = client.scrape(ScrapeConfig(
    url="https://web-scraping.dev/products",
    render_js=True,
    rendering_wait=5000,
))

# Output markdown (cho LLM)
result = client.scrape(ScrapeConfig(
    url="https://web-scraping.dev/products",
    format="markdown",
))
```

## Xử lý lỗi

```python
from scrapfly.errors import ScrapflyThrottleError, UpstreamHttpClientError

try:
    result = client.scrape(ScrapeConfig(url="...", asp=True))
except ScrapflyThrottleError as e:
    print(f"Rate limited, retry after {e.retry_delay}s")
except UpstreamHttpClientError as e:
    print(f"Target returned 4xx: {e.message}")
```

## Khi nào dùng

✅ Trang bị Cloudflare, DataDome, bot detection mạnh
✅ Cần proxy rotation + fingerprinting tự động
✅ Trang cần JavaScript render phức tạp
