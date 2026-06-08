# TASK-FE-POSTING-01: Danh sách bài đăng

**Module**: Posting Management
**Layer**: Frontend — Vue 3
**Priority**: P0
**Depends on**: TASK-BE-POSTING-03

---

## Mục tiêu

Trang danh sách bài đăng với table view, filter, search, pagination.

## Checklist

- [ ] PostListView.vue (views/posting/)
- [ ] PostTable.vue component
- [ ] PostFilterBar.vue component
- [ ] usePostStore.ts (Pinia store)
- [ ] postService.ts (API service)
- [ ] post.ts (TypeScript types)
- [ ] Router: `/posting` → PostListView
- [ ] Sidebar menu item "Đăng bài"

## UI Requirements

- Table hiển thị: Title, Status (badge màu), Channel count, Author, PublishedAt, Actions
- Filter: theo Status, Author, khoảng thời gian
- Search: theo Title
- Pagination: Element Plus el-pagination
- Actions: View, Edit, Delete (theo permission)
- Loading states cho API calls

## Verification

- Danh sách hiển thị đúng
- Filter/Search hoạt động
- Pagination hoạt động
- Responsive trên tablet (≥768px)
