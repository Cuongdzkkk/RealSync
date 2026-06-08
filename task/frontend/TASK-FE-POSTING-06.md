# TASK-FE-POSTING-06: Analytics Dashboard

**Module**: Posting Management
**Layer**: Frontend — Vue 3
**Priority**: P1
**Depends on**: TASK-BE-POSTING-06, TASK-FE-POSTING-01

---

## Mục tiêu

Dashboard analytics cho module Posting — charts, metrics, conversion tracking.

## Checklist

- [ ] PostAnalyticsView.vue (views/posting/)
- [ ] PostMetricsCards.vue component (tổng Views, Clicks, Leads, Conversion)
- [ ] PostPerformanceChart.vue component (ECharts)
- [ ] ChannelComparisonChart.vue component
- [ ] Router: `/posting/analytics` → PostAnalyticsView

## UI Requirements

- Metrics cards: Total Views, Total Clicks, Leads Generated, Avg Conversion Rate
- Performance chart: line/bar chart theo thời gian
- Channel comparison: so sánh hiệu suất giữa các kênh
- Top posts: bảng top 10 bài đăng hiệu suất cao nhất
- Date range filter: lọc theo khoảng thời gian

## Verification

- Charts render đúng dữ liệu
- Filter date range hoạt động
- Responsive trên tablet
