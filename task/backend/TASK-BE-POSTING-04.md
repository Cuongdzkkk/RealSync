# TASK-BE-POSTING-04: Post Channels API

**Module**: Posting Management
**Layer**: Backend — Api, Services
**Assignee**: Cường
**Priority**: P1
**Depends on**: TASK-BE-POSTING-03

---

## Mục tiêu

API quản lý kênh đăng bài (publish channels) cho mỗi post.

## Checklist

- [x] IPostChannelService interface
- [x] PostChannelService implementation
- [x] PostChannelsController hoặc nested trong PostsController
- [x] PostChannelCreateRequest DTO
- [x] PostChannelResponse DTO
- [x] DI registration

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/posts/{postId}/channels` | Lấy channels của post | ✅ |
| `POST` | `/api/v1/posts/{postId}/channels` | Thêm channel | ✅ Manager+ |
| `PUT` | `/api/v1/posts/{postId}/channels/{id}` | Cập nhật channel | ✅ Manager+ |
| `DELETE` | `/api/v1/posts/{postId}/channels/{id}` | Xóa channel | ✅ Manager+ |
| `POST` | `/api/v1/posts/{postId}/channels/{id}/publish` | Publish lên kênh | ✅ Manager+ |

## Verification

- [x] Thêm/xóa channel cho post hoạt động
- [x] Publish action cập nhật PublishStatus
- [x] Error handling khi publish fail
