import asyncio
import sys
import json
import os
import argparse
from crawl4ai import AsyncWebCrawler, BrowserConfig, CrawlerRunConfig
from crawl4ai.extraction_strategy import JsonCssExtractionStrategy

def parse_args():
    parser = argparse.ArgumentParser(description="RealSync Crawl4AI Helper")
    parser.add_argument("--url", required=True, help="Target URL to crawl")
    parser.add_argument("--jobId", default="crawl4ai-job", help="Unique ID for the job")
    return parser.parse_args()

async def main():
    args = parse_args()
    print(f"[Crawl4AI Helper] Bắt đầu cào URL: {args.url}")

    # Cấu hình Browser Stealth để tránh bị chặn
    browser_config = BrowserConfig(
        enable_stealth=True,
        headless=False, # Headed mode giúp bypass bot detection dễ hơn ở local
        browser_type="chromium"
    )

    # Chiến lược trích xuất cấu trúc tin đăng BĐS
    schema = {
        "name": "Real Estate Listing Extractor",
        "baseSelector": "div, article, section", # Quét các khối tin
        "fields": [
            {
                "name": "title",
                "selector": "h1, h2, h3, a[class*='title'], [class*='title']",
                "type": "text"
            },
            {
                "name": "price",
                "selector": "[class*='price'], [itemprop='price']",
                "type": "text"
            },
            {
                "name": "description",
                "selector": "p, [class*='description'], [itemprop='description']",
                "type": "text"
            }
        ]
    }
    
    extraction_strategy = JsonCssExtractionStrategy(schema, verbose=True)
    run_config = CrawlerRunConfig(
        extraction_strategy=extraction_strategy,
        cache_mode="BYPASS"
    )

    listings = []
    success = False

    try:
        async with AsyncWebCrawler(config=browser_config) as crawler:
            result = await crawler.arun(url=args.url, config=run_config)
            
            if result.success:
                success = True
                print("[Crawl4AI Helper] ✅ Cào thành công!")
                # Parse kết quả trích xuất dạng JSON
                try:
                    if result.extracted_content:
                        extracted = json.loads(result.extracted_content)
                        if isinstance(extracted, list):
                            for idx, item in enumerate(extracted[:10]):
                                # Chuẩn hóa dữ liệu tương thích với RealSync backend DTOs
                                listings.append({
                                    "title": item.get("title", f"Tin đăng BĐS {idx+1}").strip(),
                                    "price": item.get("price", "Thỏa thuận").strip(),
                                    "size": "Không rõ",
                                    "description": item.get("description", "").strip(),
                                    "contactName": "Người đăng",
                                    "contactPhone": "Không rõ",
                                    "sourceUrl": args.url,
                                    "images": []
                                })
                except Exception as e:
                    print(f"[Crawl4AI Helper] Lỗi parse JSON content: {str(e)}")
            else:
                print(f"[Crawl4AI Helper] ❌ Cào thất bại: {result.error_message}")
    except Exception as ex:
        print(f"[Crawl4AI Helper] ❌ Xảy ra lỗi ngoại lệ: {str(ex)}")

    # Ghi nhận kết quả ra file JSON cho backend C# đọc
    results_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "results"))
    if not os.path.exists(results_dir):
        os.makedirs(results_dir, exist_ok=True)
    
    file_path = os.path.join(results_dir, f"{args.jobId}.json")
    
    final_output = {
        "success": success and len(listings) > 0,
        "totalFound": len(listings),
        "listings": listings,
        # Default first item
        "title": listings[0]["title"] if listings else "Crawl4AI Scan",
        "description": listings[0]["description"] if listings else "Không tìm thấy dữ liệu.",
        "price": listings[0]["price"] if listings else "Không rõ",
        "size": "Không rõ",
        "contactName": "Không rõ",
        "contactPhone": "Không rõ",
        "images": [],
        "sourceUrl": args.url
    }

    with open(file_path, "w", encoding="utf-8") as f:
        json.dump(final_output, f, ensure_ascii=False, indent=2)
    
    print(f"[Crawl4AI Helper] Kết quả đã lưu tại: {file_path}")

if __name__ == "__main__":
    asyncio.run(main())
