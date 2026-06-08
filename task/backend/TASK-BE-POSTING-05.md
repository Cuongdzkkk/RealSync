# TASK-BE-POSTING-05: Schedule Posting API

**Module**: Posting Management
**Layer**: Backend — Api, Services
**Assignee**: Cường
**Priority**: P1
**Depends on**: TASK-BE-POSTING-03

---

## Mục tiêu

API lên lịch đăng bài tự động.

## Checklist

- [x] IPostScheduleService interface
- [x] PostScheduleService implementation
- [x] ScheduleCreateRequest DTO
- [x] ScheduleResponse DTO
- [ ] Hangfire integration (API ready, Hangfire integration pending)
- [x] DI registration

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/posts/{postId}/schedules` | Lấy lịch của post | ✅ |
| `POST` | `/api/v1/posts/{postId}/schedules` | Lên lịch đăng | ✅ Manager+ |
| `DELETE` | `/api/v1/posts/{postId}/schedules/{id}` | Hủy lịch | ✅ Manager+ |
| `GET` | `/api/v1/post-schedules/upcoming` | Lịch sắp tới (all posts) | ✅ |

## Verification

- [x] Lên lịch thành công
- [x] Hủy lịch cập nhật status = Cancelled
- [ ] Background job chạy đúng giờ (test manual - Hangfire pending)
