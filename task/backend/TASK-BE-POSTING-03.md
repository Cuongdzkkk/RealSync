# TASK-BE-POSTING-03: CRUD Posts API

**Module**: Posting Management
**Layer**: Backend — Api, Services, Core
**Assignee**: Cường
**Priority**: P0
**Depends on**: TASK-BE-POSTING-02

---

## Mục tiêu

Xây dựng CRUD API cho Posts.

## Checklist

- [x] IPostService interface
- [x] PostService implementation
- [x] PostsController (kế thừa BaseController)
- [x] PostCreateRequest DTO
- [x] PostUpdateRequest DTO
- [x] PostResponse DTO
- [x] PostFilterRequest DTO (search, status, authorId, propertyId)
- [x] PostCreateValidator (FluentValidation)
- [x] DI registration

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/posts` | Danh sách posts (paginated) | ✅ |
| `GET` | `/api/v1/posts/{id}` | Chi tiết post | ✅ |
| `POST` | `/api/v1/posts` | Tạo post mới | ✅ Manager+ |
| `PUT` | `/api/v1/posts/{id}` | Cập nhật post | ✅ Manager+ |
| `PATCH` | `/api/v1/posts/{id}/status` | Đổi trạng thái | ✅ Manager+ |
| `DELETE` | `/api/v1/posts/{id}` | Xóa mềm | ✅ Admin |

## Verification

- [x] Swagger hiển thị đầy đủ endpoints
- [x] CRUD hoạt động đúng
- [x] Pagination hoạt động
- [x] Soft delete hoạt động
- [x] Validation trả lỗi đúng format
