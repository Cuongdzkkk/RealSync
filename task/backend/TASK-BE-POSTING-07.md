# TASK-BE-POSTING-07: AI Content API

**Module**: Posting Management
**Layer**: Backend — Api, Services
**Assignee**: Cường
**Priority**: P1
**Depends on**: TASK-BE-POSTING-03

---

## Mục tiêu

API tạo nội dung bài đăng bằng AI.

## Checklist

- [x] IAIContentService interface
- [x] AIContentService implementation (Mock content ready)
  - [ ] AI Engine integration (Content AI Engine - M6 integration pending)
- [x] AIContentGenerateRequest DTO (prompt, propertyId)
- [x] AIContentGenerationResponse DTO
- [x] DI registration

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/v1/posts/{postId}/ai-content/generate` | Generate nội dung AI | ✅ Manager+ |
| `GET` | `/api/v1/posts/{postId}/ai-content` | Lịch sử generation | ✅ |
| `GET` | `/api/v1/posts/{postId}/ai-content/{id}` | Chi tiết generation | ✅ |

## Notes

- Tích hợp với AI module (Content AI Engine — M6)
- Fallback: nếu AI service unavailable, trả lỗi graceful
- Log token usage cho cost tracking

## Verification

- [x] Generate trả nội dung từ AI (mock content tested)
- [x] Lịch sử generation lưu đúng
- [ ] Error handling khi AI service down (integration pending)
