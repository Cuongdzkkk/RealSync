---
name: scrapfly-browser
description: Automate cloud browsers với Playwright qua Scrapfly Cloud Browser API. Proxy rotation, anti-bot fingerprinting, geo-targeting built-in.
---

# Scrapfly Cloud Browser — Remote Browser Automation

**Nguồn:** https://github.com/scrapfly/skills

Điều khiển browser từ xa với:
- ✅ Anti-bot fingerprinting tự động (TLS, WebGL, Canvas, HTTP/2)
- ✅ Proxy rotation + geo-targeting (datacenter/residential)
- ✅ Session persistence (login state giữa các lần kết nối)
- ✅ Screenshot, file download

## Cài đặt

```bash
pip install playwright
export SCRAPFLY_API_KEY="your-api-key"
```

## Kết nối cơ bản

```python
from playwright.sync_api import sync_playwright
import os

API_KEY = os.environ["SCRAPFLY_API_KEY"]
WS_URL = f"wss://browser.scrapfly.io?api_key={API_KEY}&proxy_pool=datacenter&os=linux"

with sync_playwright() as p:
    browser = p.chromium.connect_over_cdp(WS_URL)
    try:
        context = browser.contexts[0]
        page = context.pages[0] if context.pages else context.new_page()
        page.goto("https://web-scraping.dev")
        print("Title:", page.title())
    finally:
        browser.close()
```

## Với residential proxy + geo-target

```python
WS_URL = f"wss://browser.scrapfly.io?api_key={API_KEY}&proxy_pool=residential&country=de&os=windows"
```

## Session persistent (giữ đăng nhập)

```python
# Lần 1: Login
WS_URL = f"wss://browser.scrapfly.io?api_key={API_KEY}&session=my-session&auto_close=false"
# ... login flow ...
browser.close()  # session vẫn còn

# Lần 2: Dùng lại session
WS_URL = f"wss://browser.scrapfly.io?api_key={API_KEY}&session=my-session"
# ... đã login sẵn ...
```

## Khi nào dùng

✅ Cần Playwright đầy đủ nhưng muốn proxy + fingerprint tự động
✅ Cần login flow phức tạp, giữ session
✅ Cần geo-targeting
