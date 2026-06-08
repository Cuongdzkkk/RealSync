# TASK-FE-POSTING-02: Chi tiết bài đăng

**Module**: Posting Management
**Layer**: Frontend — Vue 3
**Priority**: P0
**Depends on**: TASK-FE-POSTING-01

---

## Mục tiêu

Trang chi tiết bài đăng — hiển thị nội dung, trạng thái channels, analytics.

## Checklist

- [ ] PostDetailView.vue (views/posting/)
- [ ] PostContent.vue component (preview nội dung)
- [ ] PostChannelStatus.vue component (trạng thái từng kênh)
- [ ] PostAnalyticsSummary.vue component (mini analytics)
- [ ] Router: `/posting/:id` → PostDetailView

## UI Requirements

- Header: Title, Status badge, Author, ngày tạo
- Content preview: render HTML/markdown
- Thumbnail hiển thị
- Channel status cards: từng kênh hiển thị trạng thái (Pending/Published/Failed)
- Analytics mini: Views, Clicks, Leads, Conversion Rate
- Action buttons: Edit, Publish, Schedule, Delete

## Verification

- Chi tiết hiển thị đầy đủ
- Channel status cập nhật realtime nếu có SignalR
- Analytics hiển thị đúng
