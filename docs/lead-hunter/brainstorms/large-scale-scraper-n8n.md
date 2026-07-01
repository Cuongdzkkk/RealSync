---
type: brainstorm
feature: lead-hunter
idea_slug: large-scale-scraper-n8n
status: draft
mode: deep
lang: vi
owner: Antigravity-AI
created: 2026-07-01
updated: 2026-07-01
complexity_flags: [has_external_redirect, has_async_flow, has_throttle_rules, has_state_machine]
links: []
tags: [brainstorm, lead-hunter, crawler, n8n]
stale_reason: ""
changelog:
  - 2026-07-01 | /brainstorm | Thiết lập cấu trúc brainstorm ban đầu cho Hệ thống cào dữ liệu lớn tích hợp n8n và cải tiến Auto-Publishing.
---

# Hệ Thống Cào Dữ Liệu Khách Hàng Diện Rộng & Đăng Tin Tự Động Tích Hợp n8n

> Feature: lead-hunter | Idea: large-scale-scraper-n8n
> Đây là bản thảo phân tích nghiệp vụ (IT-BA Brainstorm) nhằm nâng cấp máy cào thông tin khách hàng tiềm năng (Leads) diện rộng và tích hợp tự động hóa qua n8n.

## 1. Idea Seed

Sếp yêu cầu cải tiến máy cào thông tin để hoạt động thông minh hơn: quét Leads theo từ khóa, khu vực, lọc các bài viết đã qua kiểm duyệt trên các sàn BĐS (Chợ Tốt, Batdongsan.com.vn) để thu thập thông tin khách hàng có nhu cầu thực tế (không chỉ giới hạn ở tin đăng BĐS). Đồng thời, tích hợp với n8n để tự động hóa quy trình đẩy tin, đồng bộ dữ liệu và mở rộng quy mô cào dữ liệu lớn (khủng hơn) kết hợp đăng tin tự động trên đa kênh.

## 2. Context

- **Tại sao cần thực hiện?** Hiện tại máy cào chạy đơn lẻ qua Puppeteer dễ bị block IP nếu chạy tần suất cao, giao diện chưa linh hoạt để sếp chọn lọc Leads theo nhu cầu thực tế. Việc kết nối n8n sẽ giúp sếp tự setup các kịch bản chạy định kỳ (scheduled cron jobs), kết nối webhook và phân phối Leads về CRM hiệu quả mà không cần code lại backend liên tục.
- **Mục tiêu chính:** 
  1. Tách biệt rõ ràng 2 khu vực cào: "Cào thông tin BĐS bán/thuê" và "Cào nhu cầu khách mua/thuê (Leads)".
  2. Xây dựng cơ chế vượt rào cản chống cào (Anti-bot Evasion) bằng proxy xoay vòng và Playwright Stealth.
  3. Cung cấp API Webhook để n8n kích hoạt tiến trình cào hoặc nhận kết quả Leads ngay khi phát hiện tin đăng phù hợp.

## 3. User Types (preliminary)

| User Type | Pain Point | Primary Need |
|-----------|-----------|--------------|
| Nhà quản trị / Môi giới (Sếp) | Bị chặn IP khi cào hàng loạt, dữ liệu Lead rác nhiều, không phân biệt được khách có nhu cầu thực và tin đăng ảo. | Muốn quét lead tự động 24/7 theo bộ lọc khu vực/từ khóa, đẩy thẳng Lead chất lượng về CRM, và cấu hình tự động đăng tin qua n8n. |
| Hệ thống tự động (n8n Workflow) | Cần API chuẩn để trigger crawler và webhook nhận dữ liệu Lead thời gian thực mà không bị nghẽn. | API endpoint phản hồi nhanh, ổn định, định dạng dữ liệu JSON chuẩn hóa, dễ dàng tích hợp node. |

## 4. Capabilities Breakdown

### P0 — must have
- Tách luồng cào Leads khách hàng (nhà đất cần mua/cần thuê) và cào tin đăng BĐS chính chủ trên Chợ Tốt và Batdongsan.
- Cung cấp nút Bật/Tắt bộ lọc vị trí và bộ lọc theo từ khóa tự do trên giao diện để sếp dễ dàng chuyển đổi giữa quét toàn quốc bằng AI và quét định hướng khu vực.
- Tích hợp Gemini chấm điểm trùng khớp nhu cầu khách hàng (Leads) dựa trên Prompt tùy chọn của sếp trước khi lưu vào CRM.
- API Endpoint nhận yêu cầu cào từ n8n và API Webhook đẩy Lead mới phát hiện sang n8n.

### P1 — should have
- Cơ chế Playwright Stealth + User-Agent Spoofing để hạn chế tối đa việc bị Batdongsan/Chotot block profile Chrome.
- Tự động phát hiện số điện thoại khách hàng ẩn trong ảnh/tin đăng bằng OCR hoặc quét link API ẩn của sàn.
- Giao diện giám sát Crawler (Crawler Monitor Dashboard) hiển thị số lượng tin quét được, số lead hợp lệ, số lượt lỗi và trạng thái proxy.

### P2 — nice to have
- Cơ chế Proxy Rotation (xoay vòng Proxy) tự động để cào lượng lớn dữ liệu mà không sợ bị dính Cloudflare.
- Đồng bộ đa kênh đăng bài tự động thông qua webhook n8n (gửi bài đăng sang n8n để n8n tự phân phối lên Group Facebook, Zalo, Telegram).

## 5. Core Flows (Happy Path)

### 5.1 Luồng Cào Leads Khách Hàng Tự Động & Đẩy Sang n8n

1. Sếp cấu hình chiến dịch cào Leads trên giao diện: từ khóa ("cần mua nhà Quận 10"), chọn có lọc khu vực hay dùng Prompt AI chấm điểm, và điền URL Webhook của n8n.
2. Hệ thống chạy crawler ngầm định kỳ bằng Playwright, sử dụng Profile biệt lập để cào danh sách tin đăng mua/thuê mới nhất trên các sàn.
3. System trích xuất nội dung tin đăng, dùng Gemini chấm điểm khớp với Prompt của sếp (ví dụ: điểm khớp >= 70%).
4. System lưu Lead hợp lệ vào cơ sở dữ liệu CRM và tự động POST dữ liệu Lead đó sang Webhook n8n để n8n thực hiện các bước chăm sóc tiếp theo (gửi SMS, tele-sales, hoặc lưu Google Sheets).

```
[ Sếp/n8n Trigger ] ───> [ Crawler Service ] ───> [ Quét Sàn BĐS (Stealth Mode) ]
                                                            │
                                                            ▼
[ CRM Leads DB ] <─── [ Đạt điểm >= 70% ] <─── [ Gemini Chấm Điểm Khớp ]
       │
       ▼
[ Webhook POST ] ───> [ n8n Workflow ] ───> [ Auto-SMS / Tele-sales/ Sheets ]
```

### 5.2 Luồng Đăng Bài Đa Kênh Qua Tích Hợp n8n

1. Sếp tạo hoặc biên soạn bài viết trên RealSync (có nút Lưu nội dung và Đăng bài).
2. Khi sếp bấm "Đăng bài (Publish)" và chọn kênh n8n, RealSync chuẩn bị payload thông tin bài viết (tiêu đề, nội dung, hình ảnh, video).
3. System gọi API Webhook n8n được cấu hình sẵn cho kênh này.
4. n8n tiếp nhận payload, tự động chạy kịch bản phân phối bài đăng này lên các Group Facebook, Zalo, Telegram, và trả trạng thái (Thành công/Lỗi) về RealSync để ghi nhận nhật ký.

```
[ Sếp bấm Đăng bài ] ───> [ Auto-Save nội dung vào DB ] ───> [ POST sang n8n Webhook ]
                                                                     │
                                                                     ▼
[ RealSync Monitor ] <─── [ Trả trạng thái Publish ] <─── [ n8n tự động post đa kênh ]
```

## 6. System Behavior Deep Dive

### 6.1 Decision Points

| ID | Flow | Khi nào | YES (nhánh đồng ý) | NO (nhánh từ chối) |
|---|---|---|---|---|
| D1 | Luồng cào Leads | Kiểm tra switch "Sử dụng bộ lọc vị trí" | Chỉ cào các tin có vị trí trùng khớp Tỉnh/Quận đã chọn trên map. | Cào toàn bộ tin mới nhất trên sàn, dựa hoàn toàn vào Prompt AI để lọc nhu cầu. |
| D2 | Lưu Leads vào CRM | Điểm khớp do Gemini đánh giá đạt yêu cầu? | Điểm số >= 70: Tạo bản ghi Lead mới, gửi Webhook sang n8n. | Điểm số < 70: Bỏ qua tin, ghi log phân tích để sếp theo dõi. |
| D3 | Vượt rào chống bot | Phát hiện trang Cloudflare / Captcha | Kích hoạt Undetected Playwright, đổi User-Agent và xoay IP Proxy. | Tiếp tục cào bình thường bằng profile mặc định. |

### 6.2 Scenario Matrix (nếu multi-role / đa trạng thái)

| From State | To State | Rule | Action | Result |
|------------|----------|------|--------|--------|
| Chưa quét | Đang cào | Khi có yêu cầu từ UI hoặc n8n gọi API. | Khởi tạo browser Playwright, mở tab ẩn danh. | Trạng thái máy cào chuyển sang "Running". |
| Đang cào | Đã hoàn thành | Quét hết số trang yêu cầu mà không bị chặn. | Giải phóng browser, tổng hợp số lead trích xuất. | Trạng thái chuyển sang "Success", ghi logs. |
| Đang cào | Lỗi / Bị chặn | Phát hiện IP bị cấm hoặc Cloudflare không vượt qua được sau 3 lần thử. | Giải phóng browser, lưu lỗi chi tiết vào lịch sử cào. | Trạng thái chuyển sang "Failed", gửi cảnh báo UI. |

### 6.3 State Transitions (State Machine của Máy Cào)

```
CrawlerJob: Idle → Running → Completed
                       └──→ Failed
```

| Entity | Từ | Sang | Trigger | Quay lại được? |
|--------|------|----|---------|-------------|
| CrawlerJob | Idle | Running | Bấm nút chạy trên UI hoặc n8n gọi Webhook. | Không |
| CrawlerJob | Running | Completed | Hoàn thành cào và lọc tin thành công. | Không |
| CrawlerJob | Running | Failed | Bị chặn IP hoặc sếp bấm dừng đột ngột. | Có (Bấm chạy lại) |

### 6.4 Interrupted Transactions

| Tình huống | Hệ thống còn lại gì | Resume | Cleanup |
|---|---|---|---|
| Mất kết nối internet giữa chừng | CrawlerJob ghi nhận trạng thái "Failed" với lý do mất kết nối. | Sếp hoặc n8n trigger lại job; crawler sẽ bỏ qua các link tin đã cào trong ngày. | Tự động đóng tiến trình Chromium chạy ngầm để không bị rác RAM. |
| Gemini API lỗi/hết quota | Tin đăng vẫn được lưu dạng thô (Raw Lead) nhưng ghi chú "Chưa phân tích AI". | Cho phép sếp bấm "Phân tích lại bằng AI" thủ công từ danh sách Lead thô. | Giữ nguyên tin thô, không xóa dữ liệu gốc của khách hàng. |
| n8n Webhook bị timeout | Lead vẫn lưu thành công vào RealSync CRM, trạng thái gửi webhook đổi thành "Failed". | Hệ thống tự động thử lại (Retry) gửi webhook 3 lần sau mỗi 5 phút. | Sau 3 lần fail, ghi nhận trạng thái gửi n8n là "Error" để sếp bấm gửi lại bằng tay. |

## 7. Validation, Limits & Wording

### 7.1 Validation rules

| Field | Rule |
|---|---|
| Prompt AI lọc Leads | Không được để trống nếu tắt bộ lọc vị trí. Độ dài tối thiểu 10 ký tự. |
| Webhook URL của n8n | Phải bắt đầu bằng `http://` hoặc `https://`. Phải là định dạng URL hợp lệ. |
| Vùng lọc vị trí | Phải chọn ít nhất 1 Tỉnh/Thành nếu bật chế độ "Sử dụng bộ lọc vị trí". |

### 7.2 Limits & Quotas

| Tham số | Giá trị | Window | Behavior khi vượt |
|---|---|---|---|
| Số luồng crawler chạy song song | 2 luồng | Toàn thời gian | Xếp hàng chờ (Queue) các tiến trình cào sau để tránh quá tải CPU/RAM máy sếp. |
| Tần suất request lên sàn BĐS | Tối đa 1 request / 3 giây | Mỗi luồng cào | Tự động chèn độ trễ ngẫu nhiên (Random Delay) từ 2 đến 5 giây để giả lập người dùng thật. |
| Số lượng tin quét tối đa mỗi lượt | 100 tin | Mỗi chiến dịch | Tự động dừng cào và chuyển sang bước phân tích AI để tránh bị sàn phát hiện quét bất thường. |

### 7.3 Wording samples (exact strings)

#### Error messages

| Tình huống | Wording | Code |
|---|---|---|
| Tắt lọc vị trí nhưng trống Prompt | "Vui lòng nhập Prompt AI để chấm điểm Leads do bộ lọc vị trí đã được tắt." | E-LEAD-001 |
| Sàn chặn IP (Cloudflare block) | "Không thể truy cập trang web mục tiêu. Sàn đăng tin đang chặn kết nối hoặc yêu cầu xác thực người dùng." | E-CRAWL-403 |
| Lỗi lưu Lead trùng số điện thoại | "Khách hàng với số điện thoại {phone} đã tồn tại trong CRM." | E-DB-002 |

#### Success messages

| Tình huống | Wording |
|---|---|
| Cào thông tin hoàn tất | "Đã cào dữ liệu thành công. Quét được {total} tin đăng, tìm thấy {leads} khách hàng tiềm năng phù hợp." |
| Lưu bài viết thành công | "Nội dung bài viết đã được cập nhật thành công vào cơ sở dữ liệu." |

#### Info / neutral messages

| Tình huống | Wording |
|---|---|
| Đang chạy phân tích AI | "Đang phân tích tin đăng bằng AI Gemini (Điểm khớp hiện tại: {score}%)..." |
| Đang đẩy webhook n8n | "Đang gửi dữ liệu bài viết sang kịch bản n8n..." |

## 8. Assumptions

- **Môi trường hoạt động:** Máy cào chạy trên cùng một máy local của sếp (sử dụng IP mạng nhà/văn phòng của sếp), do đó profile đăng nhập sàn sẽ được lưu trực tiếp trên Chrome của máy sếp.
- **n8n Hosting:** Sếp tự cài đặt và host n8n (ở local hoặc VPS) và cung cấp đường dẫn Webhook dạng tĩnh cho RealSync.
- **Tài khoản AI:** Sử dụng cấu hình API Key Gemini của hệ thống để phân tích và chấm điểm Leads.

## 9. Risks

| Rủi ro | Khả năng | Hậu quả nghiệp vụ | Cách phòng |
|--------|----------|-------------------|-----------|
| **Cấu trúc web sàn BĐS thay đổi** | Thỉnh thoảng | Không cào được tin, máy cào báo lỗi liên tục, gián đoạn nguồn Leads mới. | Thiết kế crawler dạng modular, cấu hình selector DOM ở file JSON riêng để cập nhật nhanh không cần build lại backend. |
| **Tài khoản sàn bị khóa do cào quá nhanh** | Thường | Mất tài khoản đăng tin/quét tin, phải tạo tài khoản mới. | Sử dụng Stealth mode, thiết lập random delay cao và giới hạn số trang quét mỗi lượt giống hành vi người dùng thật. |
| **n8n webhook bị gián đoạn** | Hiếm | Leads cào được không thể đồng bộ sang hệ thống chăm sóc khách hàng khác. | Lưu trữ dữ liệu Leads tại CRM của RealSync trước rồi mới đẩy webhook, cho phép bấm gửi lại thủ công nếu webhook lỗi. |

## 10. Success Criteria

- Thu thập được Leads khách hàng thực tế chính xác theo khu vực/từ khóa sếp cần với tỉ lệ khớp AI đánh giá trên 80%.
- Giảm thiểu tỉ lệ bị block IP xuống dưới 5% nhờ tích hợp các tham số Stealth và random delay.
- Kịch bản n8n nhận webhook hoạt động ổn định dưới 2 giây phản hồi.

## 11. Open Questions

- [ ] OQ-1: Sếp muốn nhận Lead BĐS có kèm theo thông tin chi tiết của bài đăng gốc dưới dạng file đính kèm hay chỉ cần văn bản text thô?
- [ ] OQ-2: Khi n8n thực hiện đăng bài đa kênh, sếp muốn RealSync kiểm soát trạng thái đăng của từng kênh cụ thể (Ví dụ: Facebook Group A thành công, Group B lỗi) hay chỉ cần n8n báo trạng thái chung?

## 12. Next Steps

Sau khi sếp duyệt bản phân tích brainstorm này:
- `/urd lead-hunter` — Viết tài liệu đặc tả yêu cầu người dùng chi tiết cho Lead Hunter.
- `/prd lead-hunter` — Chốt danh sách tính năng triển khai cho luồng n8n & crawler nâng cấp.
