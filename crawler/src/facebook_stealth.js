const { addExtra } = require('playwright-extra');
const playwright = addExtra(require('patchright'));
const stealth = require('puppeteer-extra-plugin-stealth')();
const fs = require('fs');
const path = require('path');

// Apply stealth plugin
playwright.use(stealth);
const { chromium } = playwright;

// ═══════════════════════════════════════════════════════════════
// Helper functions
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

async function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// Emulate human typing speed
async function humanType(page, selector, text) {
    const element = page.locator(selector).first();
    await element.focus();
    for (const char of text) {
        await element.type(char);
        await delay(randomDelay(50, 150)); // typing speed: 50-150ms per key
    }
}

// ═══════════════════════════════════════════════════════════════
// Facebook Group Crawler (fb-crawl)
// ═══════════════════════════════════════════════════════════════

async function runFacebookCrawl(page, groupUrl, maxPosts = 5) {
    console.log(`[FB Crawler] Bắt đầu cào nhóm Facebook: ${groupUrl}`);
    
    // Mở URL nhóm
    await page.goto(groupUrl, { waitUntil: 'networkidle', timeout: 90000 });
    await page.waitForTimeout(randomDelay(4000, 6000));
    
    // Cuộn trang từ từ để load thêm bài viết
    console.log('[FB Crawler] Đang cuộn trang để tải bài đăng...');
    for (let s = 0; s < 3; s++) {
        await page.evaluate(() => window.scrollBy(0, window.innerHeight * 0.8));
        await page.waitForTimeout(randomDelay(2000, 3500));
    }

    console.log('[FB Crawler] Bắt đầu trích xuất bài viết và bình luận...');
    const rawPosts = await page.evaluate(() => {
        const results = [];
        const articles = document.querySelectorAll('div[role="article"]');
        
        articles.forEach((article) => {
            // Lấy nội dung bài viết
            const msgEl = article.querySelector('div[data-ad-preview="message"], div[data-testid="post_message"], div[dir="auto"]');
            if (!msgEl) return;
            const postText = msgEl.innerText || '';

            // Tên tác giả bài viết
            let authorName = 'Người dùng ẩn danh';
            const authorEl = article.querySelector('h2 a, strong a, span[role="link"]');
            if (authorEl) authorName = authorEl.innerText || authorName;

            // Link bài viết
            let postLink = window.location.href;
            const linkEl = article.querySelector('a[href*="/posts/"], a[href*="/permalink/"], span > a[role="link"]');
            if (linkEl && linkEl.href) postLink = linkEl.href;

            // Quét các comment hiển thị trong bài viết này
            const comments = [];
            const commentEls = article.querySelectorAll('div[role="comment"], div[data-comment-id]');
            commentEls.forEach((comm) => {
                let commenterName = 'Người dùng ẩn danh';
                const commAuthorEl = comm.querySelector('span[role="link"] a, a[class*="actor"], h3 a');
                if (commAuthorEl) commenterName = commAuthorEl.innerText || commenterName;

                const commTextEl = comm.querySelector('div[dir="auto"], span[class*="text"]');
                const commentText = commTextEl ? commTextEl.innerText || '' : '';

                if (commentText) {
                    comments.push({
                        authorName: commenterName,
                        text: commentText
                    });
                }
            });

            results.push({
                authorName,
                text: postText,
                url: postLink,
                comments
            });
        });
        return results;
    });

    console.log(`[FB Crawler] Trích xuất được ${rawPosts.length} bài đăng thực tế.`);

    const listings = [];
    
    // Trích xuất số điện thoại bằng regex
    const extractPhone = (text) => {
        const match = text.match(/(0[3|5|7|8|9][0-9]{8})\b/);
        return match ? match[1] : '';
    };

    // Duyệt qua các bài viết và bình luận đã cào để tạo Leads
    for (let i = 0; i < Math.min(rawPosts.length, maxPosts); i++) {
        const post = rawPosts[i];
        
        // 1. Thêm bài viết gốc làm 1 lead tiềm năng
        const postPhone = extractPhone(post.text);
        listings.push({
            title: `Bài viết từ Group - ${post.authorName}`,
            description: post.text,
            price: 'Thỏa thuận',
            size: 'Không rõ',
            contactName: post.authorName,
            contactPhone: postPhone || `Facebook: ${post.authorName}`,
            sourceUrl: post.url,
            images: [],
            comments: []
        });

        // 2. Thêm từng comment làm các leads riêng biệt với ngữ cảnh bài viết gốc
        if (post.comments && post.comments.length > 0) {
            console.log(`     -> Bài đăng có ${post.comments.length} bình luận. Cào chi tiết...`);
            post.comments.forEach((comm) => {
                const commPhone = extractPhone(comm.text);
                listings.push({
                    title: `Bình luận của ${comm.authorName} dưới bài đăng`,
                    description: `Nội dung bình luận: "${comm.text}"\n\n(Bình luận trong bài viết gốc của ${post.authorName}: "${post.text.substring(0, 150)}...")`,
                    price: 'Thỏa thuận',
                    size: 'Không rõ',
                    contactName: comm.authorName,
                    contactPhone: commPhone || `Facebook: ${comm.authorName}`,
                    sourceUrl: post.url,
                    images: [],
                    comments: []
                });
            });
        }
    }

    console.log(`[FB Crawler] ✅ Hoàn tất! Tạo được ${listings.length} tin/bình luận lead chi tiết.`);
    return listings;
}

// ═══════════════════════════════════════════════════════════════
// Facebook Group Posting (fb-post)
// ═══════════════════════════════════════════════════════════════

async function runFacebookPost(page, groupUrl, content, mediaUrl) {
    console.log(`[FB Poster] Truy cập nhóm Facebook để đăng bài: ${groupUrl}`);
    await page.goto(groupUrl, { waitUntil: 'networkidle', timeout: 90000 });
    await page.waitForTimeout(randomDelay(4000, 6000));

    // Tìm ô "Viết gì đó..."
    const writeSelectors = [
        'span:has-text("Viết câu hỏi cho nhóm...")',
        'span:has-text("Viết gì đó...")',
        'span:has-text("Write something...")',
        'div[role="button"]:has-text("Write something...")',
        'div[role="button"]:has-text("Viết gì đó...")',
        '[class*="x1i10hfl"]:has-text("Viết gì đó...")'
    ];

    let writeBox = null;
    for (const sel of writeSelectors) {
        try {
            const loc = page.locator(sel).first();
            if (await loc.count() > 0 && await loc.isVisible()) {
                writeBox = loc;
                break;
            }
        } catch (e) {}
    }

    if (!writeBox) {
        throw new Error('Không tìm thấy ô soạn thảo "Viết gì đó..." trên giao diện Facebook Group. Hãy chắc chắn bạn đã đăng nhập ở profile.');
    }

    console.log('[FB Poster] Nhấp vào ô viết bài...');
    await writeBox.click();
    await page.waitForTimeout(randomDelay(2000, 3000));

    // Tìm ô nhập text thực sự (div contenteditable)
    const editorSelector = 'div[role="textbox"], div[contenteditable="true"]';
    await page.waitForSelector(editorSelector, { timeout: 10000 });
    
    console.log('[FB Poster] Đang giả lập gõ chữ như người thật...');
    await humanType(page, editorSelector, content);
    await page.waitForTimeout(randomDelay(1500, 2500));

    // Upload media nếu có
    if (mediaUrl && fs.existsSync(mediaUrl)) {
        console.log(`[FB Poster] Đang upload file đính kèm: ${mediaUrl}`);
        // Tìm nút chọn ảnh/video
        const photoBtn = page.locator('div[role="button"]:has-text("Ảnh/video"), div[role="button"]:has-text("Photo/video")').first();
        if (await photoBtn.count() > 0) {
            await photoBtn.click();
            await page.waitForTimeout(1500);
        }
        
        const fileInput = page.locator('input[type="file"]').first();
        if (await fileInput.count() > 0) {
            await fileInput.setInputFiles(mediaUrl);
            await page.waitForTimeout(randomDelay(4000, 6000)); // Đợi file upload xong
        }
    }

    // Tìm nút Đăng (Post)
    const postBtnSelectors = [
        'div[role="button"]:has-text("Đăng")',
        'div[role="button"]:has-text("Post")',
        'span:has-text("Đăng")'
    ];

    let postBtn = null;
    for (const sel of postBtnSelectors) {
        try {
            const loc = page.locator(sel).first();
            if (await loc.count() > 0 && await loc.isVisible()) {
                postBtn = loc;
                break;
            }
        } catch (e) {}
    }

    if (!postBtn) {
        throw new Error('Không tìm thấy nút "Đăng" bài viết.');
    }

    console.log('[FB Poster] Bấm nút Đăng bài...');
    await postBtn.click();
    
    // Đợi bài đăng được xuất bản
    console.log('[FB Poster] Đợi xử lý xuất bản bài đăng...');
    await page.waitForTimeout(randomDelay(6000, 9000));
    
    console.log('[FB Poster] ✅ Đăng bài thành công!');
    return true;
}

// ═══════════════════════════════════════════════════════════════
// Main Executor
// ═══════════════════════════════════════════════════════════════

async function main() {
    const args = parseArgs();
    const mode = args.mode || 'fb-crawl';
    const targetUrl = args.url;
    const jobId = args.jobId || 'fb-job';
    const content = args.content || '';
    const media = args.media || '';

    if (!targetUrl) {
        console.error('[FB Runner] Thiếu tham số --url');
        process.exit(1);
    }

    console.log('═══════════════════════════════════════════════════════');
    console.log('[FB Runner] Facebook Stealth Browser Automation');
    console.log(`[FB Runner] URL: ${targetUrl}`);
    console.log(`[FB Runner] Chế độ: ${mode.toUpperCase()}`);
    console.log('═══════════════════════════════════════════════════════');

    const localAppData = process.env.LOCALAPPDATA || path.join(process.env.USERPROFILE, 'AppData', 'Local');
    const defaultUserDataDir = path.join(localAppData, 'Google', 'Chrome', 'User Data RealSync');
    const sharedStatePath = path.join(__dirname, '..', 'fb_storage_state.json');
    const tempUserDataDir = path.join(localAppData, 'Google', 'Chrome', `User Data RealSync - Job - ${jobId}`);

    console.log('[FB Runner] Khởi động trình duyệt extra-stealth...');
    let context;
    let isTempProfile = false;

    try {
        context = await chromium.launchPersistentContext(defaultUserDataDir, {
            channel: 'chrome',
            headless: false, // Bắt buộc false để sếp nhìn thấy hoặc sử dụng đăng nhập thật
            viewport: { width: 1280, height: 800 },
            locale: 'vi-VN',
            args: [
                '--no-sandbox', 
                '--disable-dev-shm-usage',
                '--profile-directory=Default',
                '--no-first-run',
                '--no-default-browser-check'
            ]
        });
        console.log('[FB Runner] ✅ Đã kết nối với Chrome Profile RealSync chính.');
    } catch (err) {
        console.log('[FB Runner] ⚠️ Trình duyệt chính đang bận hoặc bị khóa. Khởi động cấu hình chạy song song (Temp Profile)...');
        isTempProfile = true;

        const launchOptions = {
            channel: 'chrome',
            headless: false,
            viewport: { width: 1280, height: 800 },
            locale: 'vi-VN',
            args: [
                '--no-sandbox', 
                '--disable-dev-shm-usage',
                '--profile-directory=Default',
                '--no-first-run',
                '--no-default-browser-check'
            ]
        };

        if (fs.existsSync(sharedStatePath)) {
            launchOptions.storageState = sharedStatePath;
        }

        context = await chromium.launchPersistentContext(tempUserDataDir, launchOptions);
        console.log(`[FB Runner] ✅ Đã kết nối với Chrome Profile song song: ${tempUserDataDir}`);
    }

    const page = context.pages().length > 0 ? context.pages()[0] : await context.newPage();
    let finalResult = { success: false, listings: [] };

    try {
        if (mode === 'fb-crawl') {
            const listings = await runFacebookCrawl(page, targetUrl, 5);
            finalResult = {
                success: listings.length > 0,
                totalFound: listings.length,
                listings: listings
            };
        } else if (mode === 'fb-post') {
            const ok = await runFacebookPost(page, targetUrl, content, media);
            finalResult = {
                success: ok,
                message: "Đăng bài Facebook Group thành công!"
            };
        }
    } catch (runErr) {
        console.error('[FB Runner] ❌ Xảy ra lỗi trong tiến trình chạy:', runErr.message);
        finalResult = {
            success: false,
            error: runErr.message
        };
    } finally {
        await page.waitForTimeout(2000);
        
        // Cập nhật lưu cookies trước khi tắt
        try {
            await context.storageState({ path: sharedStatePath });
            console.log('[FB Runner] Đã lưu/cập nhật cookies phiên đăng nhập vào bộ lưu trữ dùng chung.');
        } catch (saveErr) {
            console.log('[FB Runner] Không thể lưu cookies phiên đăng nhập:', saveErr.message);
        }

        await context.close();
        console.log('[FB Runner] Trình duyệt đã đóng.');

        // Xóa thư mục profile tạm nếu có
        if (isTempProfile && fs.existsSync(tempUserDataDir)) {
            await new Promise(resolve => setTimeout(resolve, 2000)); // Đợi tiến trình Chrome giải phóng file hoàn toàn
            try {
                fs.rmSync(tempUserDataDir, { recursive: true, force: true });
                console.log('[FB Runner] Đã dọn dẹp thư mục profile tạm.');
            } catch (rmErr) {
                console.log(`[FB Runner] Không thể dọn dẹp thư mục tạm: ${rmErr.message}`);
            }
        }
    }

    // Save results to file for C# Backend
    const resultsDir = path.join(__dirname, '..', 'results');
    if (!fs.existsSync(resultsDir)) fs.mkdirSync(resultsDir, { recursive: true });
    const filePath = path.join(resultsDir, `${jobId}.json`);
    fs.writeFileSync(filePath, JSON.stringify(finalResult, null, 2), 'utf8');
    console.log(`[FB Runner] Kết quả đã lưu tại: ${filePath}`);
}

main().catch(err => {
    console.error('[FB Runner] Fatal Error:', err.message);
    process.exit(1);
});
