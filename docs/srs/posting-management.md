# 📋 SRS — Posting Management Module

> Module quản lý đăng bài bất động sản cho hệ thống RealSync.

---

## 1. Mục tiêu

- Cho phép nhân viên tạo, quản lý và đăng bài bất động sản lên nhiều kênh
- Hỗ trợ AI tạo nội dung tự động
- Theo dõi hiệu suất bài đăng (views, clicks, leads)
- Lên lịch đăng bài tự động
- Quản lý bài đăng theo trạng thái (Draft → Scheduled → Published)

---

## 2. User Stories

### US-POST-01: Tạo bài đăng
> **Là** nhân viên kinh doanh/marketing, **tôi muốn** tạo bài đăng mới liên kết với BĐS, **để** quảng bá sản phẩm trên nhiều kênh.

**Acceptance Criteria:**
- Có thể nhập Title, Content, Summary, Thumbnail
- Có thể chọn BĐS liên kết (optional)
- Có thể chọn kênh đăng (Website, Facebook, Batdongsan, Chotot, Zalo)
- Lưu được dưới dạng Draft

### US-POST-02: Đăng bài lên kênh
> **Là** manager/marketing, **tôi muốn** publish bài lên các kênh đã chọn, **để** bài viết hiển thị trên các nền tảng.

**Acceptance Criteria:**
- Publish lên từng kênh riêng lẻ
- Theo dõi trạng thái publish (Pending, Publishing, Published, Failed)
- Xem URL bài đã publish
- Retry khi publish thất bại

### US-POST-03: AI Content Writer
> **Là** nhân viên, **tôi muốn** dùng AI để tạo nội dung bài đăng, **để** tiết kiệm thời gian viết.

**Acceptance Criteria:**
- Nhập prompt mô tả yêu cầu
- AI generate nội dung dựa trên property data
- Preview nội dung trước khi áp dụng
- Lưu lịch sử generation

### US-POST-04: Lên lịch đăng bài
> **Là** marketing, **tôi muốn** lên lịch đăng bài tự động, **để** bài đăng publish đúng giờ.

**Acceptance Criteria:**
- Chọn ngày giờ publish
- Hệ thống tự động publish khi đến giờ
- Có thể hủy lịch
- Calendar view hiển thị tất cả lịch

### US-POST-05: Theo dõi hiệu suất
> **Là** manager, **tôi muốn** xem analytics bài đăng, **để** đánh giá hiệu quả marketing.

**Acceptance Criteria:**
- Xem Views, Clicks, Leads Generated, Conversion Rate
- So sánh hiệu suất giữa các kênh
- Top bài đăng hiệu suất cao
- Filter theo thời gian

---

## 3. Business Flow

```
Tạo bài đăng (Draft)
        │
        ├── AI Content Writer (optional)
        │       ↓
        │   Generate → Preview → Apply
        │
        ├── Chọn kênh đăng
        │
        └── Action:
              ├── Save Draft
              ├── Publish Now → Publish lên channels → Track status
              └── Schedule → Chọn thời gian → Auto publish khi đến giờ
                                                     │
                                                     ▼
                                              Post Published
                                                     │
                                                     ▼
                                           Analytics tracking
                                        (Views, Clicks, Leads)
```

---

## 4. Permission Matrix

| Action | Admin | Manager | Agent (Sales) | Marketing | Viewer |
|--------|-------|---------|---------------|-----------|--------|
| Xem bài đăng | ✅ | ✅ | ✅ | ✅ | ❌ |
| Tạo bài đăng | ✅ | ✅ | ✅ | ✅ | ❌ |
| Sửa bài đăng | ✅ | ✅ | ✅ | ✅ | ❌ |
| Xóa bài đăng | ✅ | ✅ | ❌ | ✅ | ❌ |
| Publish bài | ✅ | ✅ | ❌ | ✅ | ❌ |
| Xem analytics | ✅ | ✅ | ✅ | ✅ | ❌ |
| AI Content Writer | ✅ | ✅ | ✅ | ✅ | ❌ |
| Lên lịch đăng bài | ✅ | ✅ | ❌ | ✅ | ❌ |

---

## 5. API Requirements

### Posts API
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/posts` | Danh sách posts (paginated, filtered) |
| `GET` | `/api/v1/posts/{id}` | Chi tiết post |
| `POST` | `/api/v1/posts` | Tạo post |
| `PUT` | `/api/v1/posts/{id}` | Cập nhật post |
| `PATCH` | `/api/v1/posts/{id}/status` | Đổi trạng thái |
| `DELETE` | `/api/v1/posts/{id}` | Xóa mềm |

### Channels API
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/posts/{postId}/channels` | Channels của post |
| `POST` | `/api/v1/posts/{postId}/channels` | Thêm channel |
| `POST` | `/api/v1/posts/{postId}/channels/{id}/publish` | Publish lên kênh |

### Schedule API
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/posts/{postId}/schedules` | Lịch của post |
| `POST` | `/api/v1/posts/{postId}/schedules` | Lên lịch |
| `DELETE` | `/api/v1/posts/{postId}/schedules/{id}` | Hủy lịch |

### Analytics API
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/posts/{postId}/analytics` | Analytics của post |
| `GET` | `/api/v1/post-analytics/summary` | Tổng hợp |

### AI Content API
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/posts/{postId}/ai-content/generate` | Generate AI content |
| `GET` | `/api/v1/posts/{postId}/ai-content` | Lịch sử generation |

---

## 6. UI Requirements

### Trang danh sách bài đăng (`/posting`)
- Table view với columns: Title, Status, Channels, Author, PublishedAt
- Filter/Search bar
- Pagination

### Trang chi tiết (`/posting/:id`)
- Content preview
- Channel status cards
- Analytics mini dashboard

### Form tạo/sửa (`/posting/create`, `/posting/:id/edit`)
- Rich text editor cho Content
- Property selector
- Channel checkboxes
- AI Content Writer button

### Lịch đăng bài (`/posting/schedule`)
- Calendar view (tháng/tuần)
- Schedule dialog

### Analytics (`/posting/analytics`)
- Metrics cards
- Performance charts (ECharts)
- Top posts table

---

> **Version**: 1.0.0
> **Last Updated**: 2026-06-02
