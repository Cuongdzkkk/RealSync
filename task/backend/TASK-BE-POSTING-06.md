# TASK-BE-POSTING-06: Analytics API

**Module**: Posting Management
**Layer**: Backend — Api, Services
**Assignee**: Cường
**Priority**: P1
**Depends on**: TASK-BE-POSTING-03

---

## Mục tiêu

API thống kê hiệu suất bài đăng.

## Checklist

- [x] IPostAnalyticsService interface
- [x] PostAnalyticsService implementation
- [x] PostAnalyticsResponse DTO
- [x] PostAnalyticsSummaryResponse DTO
- [x] DI registration

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/posts/{postId}/analytics` | Analytics của post | ✅ |
| `GET` | `/api/v1/post-analytics/summary` | Tổng hợp analytics | ✅ Manager+ |
| `PUT` | `/api/v1/posts/{postId}/analytics` | Cập nhật analytics | ✅ System |

## Verification

- [x] Analytics tự tạo khi post được publish
- [x] Summary trả dữ liệu aggregated đúng
- [x] ConversionRate tính toán đúng
