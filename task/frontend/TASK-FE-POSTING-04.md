# TASK-FE-POSTING-04: AI Content Writer

**Module**: Posting Management
**Layer**: Frontend — Vue 3
**Priority**: P1
**Depends on**: TASK-BE-POSTING-07, TASK-FE-POSTING-03

---

## Mục tiêu

Giao diện tạo nội dung bằng AI — nhập prompt, generate, preview, áp dụng.

## Checklist

- [ ] AIContentWriter.vue component
- [ ] AIContentHistory.vue component
- [ ] Tích hợp vào PostForm (button "Tạo bằng AI")
- [ ] aiContentService.ts (API service)

## UI Requirements

- Input prompt: textarea nhập yêu cầu
- Quick prompts: các mẫu prompt có sẵn (VD: "Viết mô tả BĐS hấp dẫn", "Tạo bài FB")
- Generate button: gọi AI API, hiển thị loading
- Preview: hiển thị nội dung AI generated
- Apply button: áp dụng nội dung vào form
- History: danh sách các lần generate trước

## Verification

- Generate nội dung từ AI thành công
- Apply nội dung vào Content field
- Loading state hiển thị đúng
- Error handling khi AI service lỗi
