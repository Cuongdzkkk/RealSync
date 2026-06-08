# TASK-FE-POSTING-03: Tạo bài đăng

**Module**: Posting Management
**Layer**: Frontend — Vue 3
**Priority**: P0
**Depends on**: TASK-FE-POSTING-01

---

## Mục tiêu

Form tạo/sửa bài đăng với chọn property, chọn channels.

## Checklist

- [ ] PostCreateView.vue (views/posting/)
- [ ] PostEditView.vue (views/posting/)
- [ ] PostForm.vue component (shared form)
- [ ] PropertySelector.vue component (chọn BĐS liên kết)
- [ ] ChannelSelector.vue component (chọn kênh đăng)
- [ ] Router: `/posting/create` → PostCreateView
- [ ] Router: `/posting/:id/edit` → PostEditView

## UI Requirements

- Form fields: Title, Content (rich text editor), Summary, Thumbnail upload
- Property selector: dropdown/search chọn BĐS liên kết (optional)
- Channel selector: checkbox chọn kênh đăng (Website, Facebook, etc.)
- Preview mode: xem trước nội dung
- Save as Draft / Publish buttons
- Validation: Title bắt buộc, Content bắt buộc khi Publish

## Verification

- Tạo bài đăng thành công
- Sửa bài đăng giữ nguyên data
- Validation hoạt động
- Thumbnail upload hoạt động
