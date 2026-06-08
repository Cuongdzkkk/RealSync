# TASK-FE-POSTING-05: Lịch đăng bài

**Module**: Posting Management
**Layer**: Frontend — Vue 3
**Priority**: P1
**Depends on**: TASK-BE-POSTING-05, TASK-FE-POSTING-01

---

## Mục tiêu

Giao diện lịch đăng bài — calendar view, quản lý schedule.

## Checklist

- [ ] PostScheduleView.vue (views/posting/)
- [ ] PostCalendar.vue component
- [ ] ScheduleDialog.vue component (dialog lên lịch)
- [ ] Router: `/posting/schedule` → PostScheduleView

## UI Requirements

- Calendar view: hiển thị các bài đăng scheduled trên lịch (theo ngày/tuần/tháng)
- Drag & drop: kéo thả để đổi ngày schedule (optional, P2)
- Schedule dialog: chọn ngày giờ, chọn channels
- Status badges: Pending (vàng), Completed (xanh), Failed (đỏ), Cancelled (xám)
- Upcoming list: sidebar danh sách bài sắp đăng

## Verification

- Calendar hiển thị schedules đúng
- Lên lịch mới hoạt động
- Hủy lịch hoạt động
- Filter theo tháng/tuần
