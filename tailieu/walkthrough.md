# Hướng dẫn Kiểm thử Hệ thống Đăng bài Tự động (Zalo & Facebook)

Tôi đã hoàn tất việc tích hợp mã nguồn đăng bài tự động trên **Zalo Group** và **Facebook Page** từ các công cụ trong thư mục `agent-skills` vào Backend của dự án. 

---

## 1. 📂 Danh sách các file đã tạo/sửa đổi
* **[PostChannelService.cs](file:///d:/A/RealSync/backend/src/RealSync.Services/Implementations/PostChannelService.cs)** [MODIFY]: Thay thế mã mô phỏng (Simulation) bằng mã gọi quy trình thực thi trực tiếp các công cụ CLI (`zalo-agent` và `publish_fb.py`).
* **[publish_fb.py](file:///d:/A/RealSync/agent-skills/social-auto-engine/publish_fb.py)** [NEW]: Tệp script Python cầu nối để nhận tham số từ C# và gọi `FacebookAPI` để post văn bản hoặc ảnh lên Facebook Page.
* **[appsettings.Development.json](file:///d:/A/RealSync/backend/src/RealSync.Api/appsettings.Development.json)** [MODIFY]: Thêm cấu hình `"Posting"` để lưu trữ mã nhận diện Group Zalo (`TargetGroupId`) phục vụ quá trình test.

---

## 2. 🚀 Lộ trình Từng Bước để Cả Nhóm vào Test

Để cả nhóm thử nghiệm tính năng đăng bài tự động trực tiếp, hãy thực hiện theo đúng các bước sau:

### PHẦN A: KIỂM THỬ ZALO GROUP
Đảm bảo bạn đã cài đặt các thư viện Node của `zalo-agent-cli`:
```bash
cd d:\A\RealSync\agent-skills\zalo-agent-cli
npm install
```

#### Bước A1: Đăng nhập tài khoản Zalo chạy Seeding
1. Mở terminal và chạy lệnh:
   ```bash
   npx zalo-agent-cli login
   ```
2. Một mã QR Code sẽ hiển thị trên màn hình. Hãy dùng điện thoại (tài khoản Zalo cá nhân dùng để test rải bài) quét mã này.
3. Khi đăng nhập thành công, session sẽ được lưu trữ an toàn trong máy tính của bạn.

#### Bước A2: Lấy Thread ID của Group Test
1. Kích hoạt chế độ lắng nghe tin nhắn:
   ```bash
   npx zalo-agent-cli listen
   ```
2. Hãy gửi một tin nhắn bất kỳ từ tài khoản khác vào Group Zalo dùng để test.
3. Terminal sẽ in ra một bản ghi JSON, tìm trường `"threadId"` (Thường có dạng `gxxxxxxxxxxxx`). Copy ID này.

#### Bước A3: Cấu hình vào dự án
1. Mở file [appsettings.Development.json](file:///d:/A/RealSync/backend/src/RealSync.Api/appsettings.Development.json).
2. Thay thế `"g123456789"` bằng `threadId` bạn vừa copy vào cấu hình `Posting:Zalo:TargetGroupId`.
3. Khởi động lại Backend API (`dotnet run`).

#### Bước A4: Bấm Đăng bài từ UI/API
1. Tạo một bài viết trong UI BĐS và chạy AI content.
2. Gọi endpoint publish của Zalo: `POST /api/v1/posts/{postId}/channels/{id}/publish`.
3. Kiểm tra Group Zalo xem bài đăng kèm hình ảnh (nếu có) đã xuất hiện chưa.

---

### PHẦN B: KIỂM THỬ FACEBOOK PAGE
Đảm bảo bạn đã cài đặt các thư viện Python cần thiết:
```bash
cd d:\A\RealSync\agent-skills\social-auto-engine
pip install -r requirements.txt
```

#### Bước B1: Tạo và cấu hình Facebook Page Token
1. Tạo một trang Facebook Page nháp/vệ tinh để test.
2. Lấy **Page Access Token** từ Graph API Explorer hoặc Portal Facebook Developer.
3. Sao chép file `.env.example` thành `.env` tại thư mục `agent-skills/social-auto-engine` và cấu hình:
   ```env
   FACEBOOK_ACCESS_TOKEN=Mã_Token_Của_Bạn
   FACEBOOK_PAGE_ID=ID_Trang_Facebook_Page
   ```

#### Bước B2: Đăng bài thử nghiệm
1. Khởi động lại Backend API (`dotnet run`).
2. Gọi endpoint publish cho Facebook: `POST /api/v1/posts/{postId}/channels/{id}/publish`.
3. Bài viết kèm hình ảnh từ database BĐS sẽ xuất hiện ngay lập tức trên dòng thời gian (feed) của Trang Facebook Page bạn đã chọn!
