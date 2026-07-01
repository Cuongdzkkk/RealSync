const { chromium } = require('patchright');
const { parse: parseHTML } = require('node-html-parser');
const https = require('https');
const http = require('http');
const fs = require('fs');
const path = require('path');

// ═══════════════════════════════════════════════════════════════
// Helper utilities
// ═══════════════════════════════════════════════════════════════

function parseArgs() {
    const args = {};
    for (let i = 2; i < process.argv.length; i++) {
        if (process.argv[i].startsWith('--')) {
            const key = process.argv[i].substring(2);
            const value = process.argv[i + 1];
            args[key] = value;
            i++;
        }
    }
    return args;
}

function randomDelay(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

// Retry logic (từ crawler-guide.md §7)
async function withRetry(fn, maxRetries = 3, baseDelay = 2000) {
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
        try {
            return await fn();
        } catch (error) {
            if (attempt === maxRetries) throw error;
            const delay = baseDelay * Math.pow(2, attempt - 1);
            console.warn(`  ⚠️ Attempt ${attempt} failed: ${error.message}. Retrying in ${delay}ms...`);
            await new Promise(resolve => setTimeout(resolve, delay));
        }
    }
}

// Simple HTTP GET (returns HTML string or null)
function httpGet(url, timeout = 15000) {
    return new Promise((resolve) => {
        const client = url.startsWith('https') ? https : http;
        const req = client.get(url, {
            headers: {
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36',
                'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
                'Accept-Language': 'vi-VN,vi;q=0.9,en;q=0.8',
                'Accept-Encoding': 'identity'
            },
            timeout
        }, (res) => {
            // Follow redirects
            if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
                httpGet(res.headers.location, timeout).then(resolve);
                return;
            }
            if (res.statusCode !== 200) {
                resolve(null);
                return;
            }
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => resolve(data));
            res.on('error', () => resolve(null));
        });
        req.on('error', () => resolve(null));
        req.on('timeout', () => { req.destroy(); resolve(null); });
    });
}

// ═══════════════════════════════════════════════════════════════
// TIER 1: Static HTTP Scraping (fast, no browser)
// Từ crawler-tiered/SKILL.md: "Try first for all URLs. Fast and free."
// ═══════════════════════════════════════════════════════════════

async function tryStaticScrape(targetUrl) {
    console.log('[Tier 1] Thử cào tĩnh bằng HTTP request...');
    const html = await httpGet(targetUrl);

    if (!html) {
        console.log('[Tier 1] ❌ Không lấy được HTML. Chuyển sang Tier 2 (Browser).');
        return null;
    }

    // Check for anti-bot indicators
    const lowerHtml = html.toLowerCase();
    if (lowerHtml.includes('captcha') || lowerHtml.includes('cloudflare') ||
        lowerHtml.includes('verify you are human') || lowerHtml.includes('challenge-platform') ||
        html.length < 2000) {
        console.log('[Tier 1] ❌ Bị chặn bởi anti-bot hoặc nội dung quá ít. Chuyển sang Tier 2.');
        return null;
    }

    const root = parseHTML(html);

    // Try to find listing detail links
    let detailLinks = [];

    if (targetUrl.includes('chotot.com') || targetUrl.includes('nhatot.com')) {
        const links = root.querySelectorAll('a[href]');
        for (const a of links) {
            const href = a.getAttribute('href');
            if (href && href.includes('.htm') && (href.includes('/mua-ban-') || href.includes('/cho-thue-'))) {
                const fullUrl = href.startsWith('http') ? href : `https://www.chotot.com${href}`;
                if (!detailLinks.includes(fullUrl)) detailLinks.push(fullUrl);
            }
        }
    } else if (targetUrl.includes('batdongsan.com.vn')) {
        const links = root.querySelectorAll('a[href]');
        for (const a of links) {
            const href = a.getAttribute('href');
            if (href && href.includes('.htm') && (href.includes('/ban-') || href.includes('/cho-thue-'))) {
                const fullUrl = href.startsWith('http') ? href : `https://batdongsan.com.vn${href}`;
                if (!detailLinks.includes(fullUrl)) detailLinks.push(fullUrl);
            }
        }
    }

    if (detailLinks.length === 0) {
        console.log(`[Tier 1] ❌ Không tìm thấy link chi tiết từ HTML tĩnh (SPA cần JS). Chuyển sang Tier 2.`);
        return null;
    }

    console.log(`[Tier 1] ✅ Tìm thấy ${detailLinks.length} link chi tiết từ HTTP tĩnh.`);

    // Try fetching detail pages statically
    const results = [];
    const maxToFetch = Math.min(detailLinks.length, 5);

    for (let i = 0; i < maxToFetch; i++) {
        const detailUrl = detailLinks[i];
        console.log(`[Tier 1] 📄 [${i + 1}/${maxToFetch}] Fetching: ${detailUrl}`);

        const detailHtml = await httpGet(detailUrl);
        if (!detailHtml || detailHtml.length < 2000) continue;

        const detailRoot = parseHTML(detailHtml);
        const listing = extractFromStaticHTML(detailRoot, detailUrl, targetUrl);
        if (listing && listing.title) {
            results.push(listing);
            console.log(`  ✅ ${listing.title} — ${listing.price}`);
        }

        // Rate limit: 2-5s (từ crawler-guide.md §6)
        await new Promise(r => setTimeout(r, randomDelay(2000, 4000)));
    }

    if (results.length === 0) {
        console.log('[Tier 1] ❌ Không trích xuất được dữ liệu từ HTML tĩnh. Chuyển sang Tier 2.');
        return null;
    }

    console.log(`[Tier 1] ✅ Thu thập thành công ${results.length} tin đăng bằng HTTP tĩnh!`);
    return { detailLinks, results };
}

function extractFromStaticHTML(root, detailUrl, baseUrl) {
    const result = {
        title: '', price: 'Thỏa thuận', size: 'Không rõ',
        description: '', contactName: 'Người đăng', contactPhone: 'Không rõ',
        images: [], sourceUrl: detailUrl
    };

    // Title
    const h1 = root.querySelector('h1');
    if (h1) result.title = h1.text.trim();

    if (detailUrl.includes('chotot.com') || detailUrl.includes('nhatot.com')) {
        const priceEl = root.querySelector('[itemprop="price"]');
        if (priceEl) result.price = priceEl.text.trim();

        const descEl = root.querySelector('[itemprop="description"]');
        if (descEl) result.description = descEl.text.trim();

        // Images from meta og:image or img tags
        const ogImage = root.querySelector('meta[property="og:image"]');
        if (ogImage) result.images.push(ogImage.getAttribute('content'));

        const imgs = root.querySelectorAll('img[src*="cdn.chotot.com"]');
        for (const img of imgs) {
            const src = img.getAttribute('src') || img.getAttribute('data-src');
            if (src && !result.images.includes(src) && !src.includes('avatar') && !src.includes('logo')) {
                result.images.push(src);
            }
        }
    } else if (detailUrl.includes('batdongsan.com.vn')) {
        const priceEl = root.querySelector('.re__pr-short-info-item .value');
        if (priceEl) result.price = priceEl.text.trim();

        const descEl = root.querySelector('.re__detail-content');
        if (descEl) result.description = descEl.text.trim().substring(0, 2000);

        const contactEl = root.querySelector('.js__agent-contact-name');
        if (contactEl) result.contactName = contactEl.text.trim();

        const phoneEl = root.querySelector('.js__agent-contact-phone');
        if (phoneEl) result.contactPhone = phoneEl.text.trim();

        const imgs = root.querySelectorAll('.re__media-thumb-item img, img[src*="file4.batdongsan.com.vn"]');
        for (const img of imgs) {
            const src = img.getAttribute('src') || img.getAttribute('data-src');
            if (src && !result.images.includes(src)) result.images.push(src);
        }
    }

    result.images = result.images.filter(Boolean).slice(0, 10);
    return result;
}

// ═══════════════════════════════════════════════════════════════
// TIER 2: Dynamic Browser Scraping (Patchright anti-detect)
// Từ crawler-tiered/SKILL.md: "Handles JavaScript-rendered content"
// ═══════════════════════════════════════════════════════════════

async function tryDynamicScrape(targetUrl, args) {
    const maxListings = parseInt(args.maxListings || '5', 10);

    console.log('[Tier 2] Khởi động Patchright browser (anti-detect)...');

    const localAppData = process.env.LOCALAPPDATA || path.join(process.env.USERPROFILE, 'AppData', 'Local');
    const defaultUserDataDir = path.join(localAppData, 'Google', 'Chrome', 'User Data RealSync');

    let context;
    try {
        context = await chromium.launchPersistentContext(defaultUserDataDir, {
            channel: 'chrome',
            headless: false,
            viewport: { width: 1280, height: 800 },
            locale: 'vi-VN',
            args: ['--no-sandbox', '--disable-dev-shm-usage']
        });
        console.log('[Tier 2] ✅ Đã mở Chrome Profile RealSync (biệt lập).');
    } catch (err) {
        console.warn('[Tier 2] ⚠️ Chrome Profile RealSync đang bận. Dùng profile crawler dự phòng...');
        const fallbackDir = path.join(localAppData, 'Google', 'Chrome', 'User Data Crawler');
        context = await chromium.launchPersistentContext(fallbackDir, {
            channel: 'chrome',
            headless: false,
            viewport: { width: 1280, height: 800 },
            locale: 'vi-VN',
            args: ['--no-sandbox', '--disable-dev-shm-usage']
        });
        console.log('[Tier 2] ✅ Đã mở Chrome Profile Crawler dự phòng.');
    }

    const page = context.pages().length > 0 ? context.pages()[0] : await context.newPage();
    const allResults = [];

    try {
        // Navigate with retry + networkidle (từ crawl4ai SKILL.md)
        console.log(`[Tier 2] Đang truy cập: ${targetUrl}`);
        await withRetry(async () => {
            await page.goto(targetUrl, { waitUntil: 'networkidle', timeout: 60000 });
        }, 3, 3000);
        await page.waitForTimeout(randomDelay(3000, 5000));

        // Collect listing links
        console.log('[Tier 2] Đang thu thập danh sách tin đăng...');
        let detailLinks = await collectDetailLinks(page, targetUrl);

        detailLinks = detailLinks.slice(0, maxListings);
        console.log(`[Tier 2] Tìm thấy ${detailLinks.length} tin đăng.`);

        if (detailLinks.length === 0) {
            console.warn('[Tier 2] Không tìm thấy link chi tiết. Cào trực tiếp trang hiện tại.');
            detailLinks = [targetUrl];
        }

        // Visit each detail page
        for (let i = 0; i < detailLinks.length; i++) {
            const detailUrl = detailLinks[i];
            console.log(`[Tier 2] 👉 [${i + 1}/${detailLinks.length}] ${detailUrl}`);

            try {
                await withRetry(async () => {
                    await page.goto(detailUrl, { waitUntil: 'networkidle', timeout: 60000 });
                }, 2, 3000);
                await page.waitForTimeout(randomDelay(2000, 4000));

                // Slow scroll to load lazy images
                await page.evaluate(async () => {
                    await new Promise((resolve) => {
                        let totalHeight = 0;
                        const distance = 150;
                        const timer = setInterval(() => {
                            const scrollHeight = document.body.scrollHeight;
                            window.scrollBy(0, distance);
                            totalHeight += distance;
                            if (totalHeight >= scrollHeight - window.innerHeight) {
                                clearInterval(timer);
                                resolve();
                            }
                        }, 80);
                    });
                });
                await page.waitForTimeout(randomDelay(1000, 2000));

                const listing = await extractListingData(page, detailUrl);
                if (listing.title) {
                    allResults.push(listing);
                    console.log(`  ✅ ${listing.title} — ${listing.price}`);
                } else {
                    console.log(`  ⚠️ Không trích xuất được dữ liệu.`);
                }
            } catch (pageErr) {
                console.error(`  ❌ Lỗi: ${pageErr.message}`);
            }

            if (i < detailLinks.length - 1) {
                await page.waitForTimeout(randomDelay(2000, 5000));
            }
        }
    } finally {
        await page.waitForTimeout(2000);
        await context.close();
        console.log('[Tier 2] Trình duyệt đã đóng.');
    }

    return allResults;
}

// Collect detail links from list page
async function collectDetailLinks(page, targetUrl) {
    const links = [];

    if (targetUrl.includes('chotot.com') || targetUrl.includes('nhatot.com')) {
        const linkEls = await page.$$('a[href]');
        for (const el of linkEls) {
            const href = await el.getAttribute('href');
            if (href && href.includes('.htm') &&
                (href.includes('/mua-ban-') || href.includes('/cho-thue-') || href.includes('nhatot.com'))) {
                const fullUrl = href.startsWith('http') ? href : `https://www.chotot.com${href}`;
                if (!links.includes(fullUrl)) links.push(fullUrl);
            }
        }
    } else if (targetUrl.includes('batdongsan.com.vn')) {
        const linkEls = await page.$$('a[href*=".htm"]');
        for (const el of linkEls) {
            const href = await el.getAttribute('href');
            if (href && (href.includes('/ban-') || href.includes('/cho-thue-'))) {
                const fullUrl = href.startsWith('http') ? href : `https://batdongsan.com.vn${href}`;
                if (!links.includes(fullUrl)) links.push(fullUrl);
            }
        }
    } else {
        const linkEls = await page.$$('a[href*=".htm"]');
        for (const el of linkEls) {
            const href = await el.getAttribute('href');
            if (href && href.match(/(ban|thue|bds|bat-dong-san|detail)/i)) {
                const fullUrl = href.startsWith('http') ? href : new URL(href, targetUrl).href;
                if (!links.includes(fullUrl)) links.push(fullUrl);
            }
        }
    }

    return links;
}

// ═══════════════════════════════════════════════════════════════
// Data extraction (Browser mode)
// ═══════════════════════════════════════════════════════════════

async function extractListingData(page, detailUrl) {
    if (detailUrl.includes('chotot.com') || detailUrl.includes('nhatot.com')) {
        return await extractChotot(page, detailUrl);
    } else if (detailUrl.includes('batdongsan.com.vn')) {
        return await extractBatdongsan(page, detailUrl);
    } else {
        return await extractGeneric(page, detailUrl);
    }
}

async function extractChotot(page, detailUrl) {
    const result = {
        title: '', price: 'Thỏa thuận', size: 'Không rõ',
        description: '', contactName: 'Người đăng', contactPhone: 'Không rõ',
        images: [], sourceUrl: detailUrl
    };

    result.title = await safeText(page, 'h1');
    result.price = await safeText(page, '[itemprop="price"], [class*="AdParam_adParamValue"]:first-of-type, [class*="price"]') || 'Thỏa thuận';
    result.size = await safeText(page, '[itemprop="size"], span[class*="size"]') || 'Không rõ';
    result.description = await safeText(page, '[itemprop="description"], [class*="AdDecription_content"], [class*="adBody"]') || '';
    result.contactName = await safeText(page, '[class*="ContactSellerName"], [class*="SellerProfile"] b, [class*="sellerName"]') || 'Người đăng';

    // Phone — click "show phone" button
    try {
        const phoneBtn = page.locator('button:has-text("Bấm để hiện"), button:has-text("hiện số"), [class*="ShowPhone"]');
        if (await phoneBtn.count() > 0) {
            await phoneBtn.first().click({ timeout: 3000 });
            await page.waitForTimeout(1500);
            const phoneText = await phoneBtn.first().innerText().catch(() => '');
            const cleaned = phoneText.replace(/[^\d]/g, '');
            if (cleaned.length >= 9) result.contactPhone = cleaned;
        }
    } catch (e) { /* ignore */ }

    // Images — multiple sources including lazy-loaded
    const imgSrcs = await page.evaluate(() => {
        const srcs = new Set();
        // Regular img tags
        document.querySelectorAll('img[src*="cdn.chotot.com"], img[src*="static.chotot.com"]').forEach(img => {
            const src = img.src || img.dataset.src;
            if (src && !src.includes('avatar') && !src.includes('logo') && !src.includes('icon')) srcs.add(src);
        });
        // Lazy-loaded with data-src
        document.querySelectorAll('img[data-src*="cdn.chotot.com"]').forEach(img => {
            if (img.dataset.src) srcs.add(img.dataset.src);
        });
        // Picture/source elements
        document.querySelectorAll('source[srcset*="cdn.chotot.com"]').forEach(src => {
            const srcset = src.srcset;
            if (srcset) srcs.add(srcset.split(' ')[0]);
        });
        // Carousel/swiper background images
        document.querySelectorAll('[style*="cdn.chotot.com"]').forEach(el => {
            const match = el.style.backgroundImage?.match(/url\(["']?([^"')]+)["']?\)/);
            if (match) srcs.add(match[1]);
        });
        return [...srcs];
    });
    result.images = imgSrcs.slice(0, 10);

    return result;
}

async function extractBatdongsan(page, detailUrl) {
    const result = {
        title: '', price: 'Thỏa thuận', size: 'Không rõ',
        description: '', contactName: 'Người đăng', contactPhone: 'Không rõ',
        images: [], sourceUrl: detailUrl
    };

    result.title = await safeText(page, 'h1.re__pr-title, h1');
    result.price = await safeText(page, '.re__pr-short-info-item .value, [class*="pr-specs-content-item"] .value') || 'Thỏa thuận';
    result.size = await safeText(page, '.re__pr-short-info-item:nth-child(2) .value') || 'Không rõ';
    result.description = await safeText(page, '.re__detail-content, [class*="detail-content"]') || '';
    result.contactName = await safeText(page, '.re__contact-name, [class*="contact-name"], .js__agent-contact-name') || 'Người đăng';

    // Phone
    try {
        const phoneBtn = page.locator('.re__btn-phone, [class*="phone"], button:has-text("Hiện số")');
        if (await phoneBtn.count() > 0) {
            const dataPhone = await phoneBtn.first().getAttribute('data-phone').catch(() => null);
            if (dataPhone) {
                result.contactPhone = dataPhone;
            } else {
                await phoneBtn.first().click({ force: true, timeout: 3000 }).catch(() => {});
                await page.waitForTimeout(1500);
                const txt = await phoneBtn.first().innerText().catch(() => '');
                const cleaned = txt.replace(/[^\d]/g, '');
                if (cleaned.length >= 9) result.contactPhone = cleaned;
            }
        }
    } catch (e) { /* ignore */ }

    // Images
    const imgSrcs = await page.evaluate(() => {
        const srcs = new Set();
        document.querySelectorAll('.re__pr-media-slide img, .re__media-thumb-item img, img[src*="file4.batdongsan.com.vn"]').forEach(img => {
            const src = img.src || img.dataset.src;
            if (src && !src.includes('avatar') && !src.includes('logo')) srcs.add(src);
        });
        document.querySelectorAll('img[data-src*="batdongsan"]').forEach(img => {
            if (img.dataset.src) srcs.add(img.dataset.src);
        });
        return [...srcs];
    });
    result.images = imgSrcs.slice(0, 10);

    return result;
}

async function extractGeneric(page, detailUrl) {
    const result = {
        title: '', price: 'Thỏa thuận', size: 'Không rõ',
        description: '', contactName: 'Người đăng', contactPhone: 'Không rõ',
        images: [], sourceUrl: detailUrl
    };

    result.title = await safeText(page, 'h1');
    result.description = await safeText(page, 'article, [class*="description"], [class*="content"]') || '';

    const imgSrcs = await page.evaluate(() => {
        const srcs = new Set();
        document.querySelectorAll('img[src*="http"]').forEach(img => {
            const src = img.src;
            if (src && !src.includes('avatar') && !src.includes('logo') && !src.includes('icon') && src.length > 30) {
                srcs.add(src);
            }
        });
        return [...srcs];
    });
    result.images = imgSrcs.slice(0, 10);

    return result;
}

async function safeText(page, selector) {
    try {
        const el = page.locator(selector).first();
        if (await el.count() > 0) {
            const text = await el.innerText({ timeout: 5000 });
            return text.replace(/\r?\n|\r/g, ' ').trim();
        }
    } catch (e) { /* ignore */ }
    return '';
}

// ═══════════════════════════════════════════════════════════════
// Main: Tiered execution
// ═══════════════════════════════════════════════════════════════

async function run() {
    const args = parseArgs();
    const targetUrl = args.url;
    const jobId = args.jobId || 'test-job';
    const area = args.area || '';
    const province = args.province || '';
    const propertyType = args.propertyType || '';
    const mode = args.mode || 'property';

    console.log('═══════════════════════════════════════════════════════');
    console.log('[Crawler] RealSync Crawler — Tiered Approach');
    console.log(`[Crawler] URL: ${targetUrl}`);
    console.log(`[Crawler] Chế độ: ${mode.toUpperCase()}`);
    console.log(`[Crawler] Khu vực: ${area}, ${province}. Loại: ${propertyType}`);
    console.log('═══════════════════════════════════════════════════════');

    if (!targetUrl) {
        console.error('[Crawler] Thiếu tham số --url');
        process.exit(1);
    }

    let allResults = [];

    // ─── TIER 1: Try Static HTTP first ───
    try {
        const staticResult = await tryStaticScrape(targetUrl);
        if (staticResult && staticResult.results.length > 0) {
            allResults = staticResult.results;
        }
    } catch (err) {
        console.warn(`[Tier 1] Exception: ${err.message}`);
    }

    // ─── TIER 2: Fall back to Browser if needed ───
    if (allResults.length === 0) {
        try {
            const dynamicResults = await tryDynamicScrape(targetUrl, args);
            allResults = dynamicResults;
        } catch (err) {
            console.error(`[Tier 2] Exception: ${err.message}`);
        }
    }

    // ─── Build & Save Result ───
    console.log(`\n═══════════════════════════════════════════════════════`);
    console.log(`[Crawler] KẾT QUẢ: Thu thập thành công ${allResults.length} tin đăng.`);
    console.log('═══════════════════════════════════════════════════════');

    const finalResult = {
        success: allResults.length > 0,
        totalFound: allResults.length,
        totalExtracted: allResults.length,
        listings: allResults,
        // Backward compat: first listing at root level
        ...(allResults.length > 0 ? {
            title: allResults[0].title,
            description: allResults[0].description,
            price: allResults[0].price,
            size: allResults[0].size,
            contactName: allResults[0].contactName,
            contactPhone: allResults[0].contactPhone,
            images: allResults[0].images,
            sourceUrl: allResults[0].sourceUrl
        } : {
            title: `${propertyType} tại ${area}, ${province}`,
            description: `Không tìm thấy dữ liệu bất động sản loại ${propertyType} tại ${area}, ${province}.`,
            price: 'Không rõ',
            size: 'Không rõ',
            contactName: 'Không rõ',
            contactPhone: 'Không rõ',
            images: [],
            sourceUrl: targetUrl
        })
    };

    const resultsDir = path.join(__dirname, '..', 'results');
    if (!fs.existsSync(resultsDir)) fs.mkdirSync(resultsDir, { recursive: true });
    const filePath = path.join(resultsDir, `${jobId}.json`);
    fs.writeFileSync(filePath, JSON.stringify(finalResult, null, 2), 'utf8');
    console.log(`[Crawler] Đã lưu kết quả tại: ${filePath}`);
}

run().catch(err => {
    console.error('[Crawler] Fatal error:', err.message);
    process.exit(1);
});
