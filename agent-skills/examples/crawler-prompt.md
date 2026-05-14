# 🕷 Crawler Prompt Template

> Dùng prompt này khi cần agent tạo crawler cho một nguồn BĐS mới.

---

## Prompt Template

```
[CRAWLER] Tạo crawler cho nguồn "{SourceName}"

Thông tin nguồn:
- URL: {baseUrl}
- Loại: {static HTML / SPA / API}
- Cần login: {Yes/No}
- Anti-bot: {Level: Low/Medium/High}

Yêu cầu:
1. Crawler class kế thừa BaseCrawler
2. CSS selectors cho list page và detail page
3. Data parser (title, price, area, address, images)
4. Rate limiting (min 3s giữa requests)
5. Error handling + retry
6. Source config trong configs/sources.json
7. Test script

Tuân thủ Crawler Rules trong SKILL.md
```

---

## Ví dụ: Crawl batdongsan.com.vn

```
[CRAWLER] Tạo crawler cho batdongsan.com.vn

Source: batdongsan.com.vn
Type: Server-rendered HTML + some JS
Anti-bot: Medium (rate limiting, CAPTCHA sometimes)
Login: Không cần

List URL pattern: /ban-nha-dat?page={page}
Detail URL pattern: /ban-nha-mat-tien-{slug}-pr{id}

Selectors (List page):
- Item container: .js__card
- Title: .js__card-title
- Price: .js__card-price
- Area: .js__card-area  
- Location: .js__card-location
- Detail link: .js__card-title a[href]
- Next page: .re__pagination-group a:last-child

Selectors (Detail page):
- Description: .re__detail-content
- Specs: .re__pr-specs-content-item
- Images: .re__media-thumb-item img[src]
- Contact name: .js__agent-contact-name
- Contact phone: .js__agent-contact-phone

Extract fields:
- title, description
- price (parse "5 tỷ", "800 triệu", "15 triệu/tháng")
- area (parse "80 m²")
- address, district, province
- bedrooms, bathrooms, floors
- images (array of URLs)
- sourceUrl, sourceId
```

---

## Crawler Implementation Example

```javascript
// crawler/src/sources/BatDongSanCrawler.js
const BaseCrawler = require('../core/BaseCrawler');

class BatDongSanCrawler extends BaseCrawler {
  getPageUrl(page) {
    return `${this.config.baseUrl}/ban-nha-dat?page=${page}`;
  }

  async parseListPage(page) {
    const items = await page.$$eval('.js__card', cards => {
      return cards.map(card => ({
        title: card.querySelector('.js__card-title')?.textContent?.trim(),
        price: card.querySelector('.js__card-price')?.textContent?.trim(),
        area: card.querySelector('.js__card-area')?.textContent?.trim(),
        location: card.querySelector('.js__card-location')?.textContent?.trim(),
        detailUrl: card.querySelector('.js__card-title a')?.href,
      }));
    });

    return items.filter(item => item.title && item.detailUrl);
  }

  async parseDetailPage(page, url) {
    await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
    await this.delay();

    const detail = await page.evaluate(() => {
      const getText = (sel) => document.querySelector(sel)?.textContent?.trim() || '';
      const getAll = (sel) => [...document.querySelectorAll(sel)].map(el => el.textContent?.trim());
      const getImages = (sel) => [...document.querySelectorAll(sel)].map(el => el.src || el.dataset.src);

      return {
        description: getText('.re__detail-content'),
        specs: getAll('.re__pr-specs-content-item'),
        images: getImages('.re__media-thumb-item img'),
        contactName: getText('.js__agent-contact-name'),
        contactPhone: getText('.js__agent-contact-phone'),
      };
    });

    return {
      ...detail,
      price: this.parsePrice(detail.price),
      area: this.parseArea(detail.area),
      sourceUrl: url,
    };
  }

  async hasNextPage(page) {
    return await page.$('.re__pagination-group a:last-child:not(.active)') !== null;
  }

  // Parse "5 tỷ" → 5000000000, "800 triệu" → 800000000
  parsePrice(priceStr) {
    if (!priceStr) return 0;
    const str = priceStr.toLowerCase().replace(/[^\d.,tỷriệu]/g, ' ').trim();
    
    if (str.includes('tỷ')) {
      const num = parseFloat(str.replace(/[^0-9.,]/g, '').replace(',', '.'));
      return Math.round(num * 1_000_000_000);
    }
    if (str.includes('triệu')) {
      const num = parseFloat(str.replace(/[^0-9.,]/g, '').replace(',', '.'));
      return Math.round(num * 1_000_000);
    }
    return parseFloat(str.replace(/[^0-9]/g, '')) || 0;
  }

  // Parse "80 m²" → 80
  parseArea(areaStr) {
    if (!areaStr) return 0;
    const match = areaStr.match(/[\d.,]+/);
    return match ? parseFloat(match[0].replace(',', '.')) : 0;
  }
}

module.exports = BatDongSanCrawler;
```

---

## Test Prompt

```
[CRAWLER] Test crawl batdongsan.com.vn

Chạy test crawl:
- Chỉ crawl 2 pages
- Log kết quả ra console
- Validate data sau crawl
- Report: total, success, error, sample data
```
