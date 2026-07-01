---
name: scrapfly-crawler
description: Crawl toàn bộ website với tự động phát hiện link, depth control, trích xuất dữ liệu có cấu trúc. Dùng khi cần crawl nhiều trang cùng lúc.
---

# Scrapfly Crawler — Site-Wide Crawling

**Nguồn:** https://github.com/scrapfly/skills

Crawl toàn bộ website với:
- ✅ Tự động phát hiện và theo link
- ✅ Giới hạn depth, số trang
- ✅ Chống bot (ASP mode)
- ✅ Export WARC/HAR
- ✅ Real-time progress tracking
- ✅ Trích xuất dữ liệu có cấu trúc

## Cài đặt

```bash
pip install scrapfly-sdk
export SCRAPFLY_API_KEY="your-api-key"
```

## Cơ bản: crawl + đợi kết quả

```python
from scrapfly import ScrapflyClient, CrawlerConfig, Crawl
import os

client = ScrapflyClient(key=os.environ["SCRAPFLY_API_KEY"])

crawl = Crawl(
    client,
    CrawlerConfig(url='https://web-scraping.dev/products', page_limit=5, content_formats=['markdown'])
).crawl().wait()

pages = crawl.warc().get_pages()
for page in pages:
    print(f"  {page['url']} ({page['status_code']})")
```

## Theo dõi tiến trình real-time

```python
import time
from scrapfly import ScrapflyClient, CrawlerConfig

client = ScrapflyClient(key=os.environ["SCRAPFLY_API_KEY"])

start_resp = client.start_crawl(CrawlerConfig(url='https://example.com', page_limit=50))

while True:
    status = client.get_crawl_status(start_resp.uuid)
    print(f"Status: {status.status} - {status.progress_pct:.1f}%")
    print(f"Crawled: {status.urls_crawled}/{status.urls_discovered}")
    if status.is_complete or status.is_failed:
        break
    time.sleep(5)
```

## Lọc path

```python
config = CrawlerConfig(
    url='https://web-scraping.dev/',
    page_limit=50,
    max_depth=5,
    include_only_paths=['*/product/*'],
    content_formats=['markdown'],
)
```

## Chống bot

```python
config = CrawlerConfig(url='https://example.com', asp=True, proxy_pool='public_residential_pool')
crawl = Crawl(client, config).crawl().wait()
```

## Khi nào dùng

✅ Cần crawl toàn bộ site hoặc 1 section
✅ Cần real-time progress tracking
✅ Cần export WARC/HAR để xử lý offline
✅ Tự động phát hiện link, theo dõi phân cấp
