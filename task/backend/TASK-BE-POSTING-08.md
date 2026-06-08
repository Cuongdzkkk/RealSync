# TASK-BE-POSTING-08: Permission & Role

**Module**: Posting Management
**Layer**: Backend — Data (Seeder)
**Assignee**: Cường
**Priority**: P0
**Depends on**: TASK-BE-POSTING-01

---

## Mục tiêu

Seed permissions và role assignments cho module Posting.

## Checklist

- [x] Thêm Role "Marketing" vào seeder
- [x] Seed posting permissions: posts.read, posts.create, posts.update, posts.delete, posts.publish
- [x] Gán permissions theo role:

## Permission Matrix

| Permission | Admin | Manager | Agent (Sales) | Marketing | Viewer |
|------------|-------|---------|---------------|-----------|--------|
| posts.read | ✅ | ✅ | ✅ | ✅ | ❌ |
| posts.create | ✅ | ✅ | ✅ | ✅ | ❌ |
| posts.update | ✅ | ✅ | ✅ | ✅ | ❌ |
| posts.delete | ✅ | ✅ | ❌ | ✅ | ❌ |
| posts.publish | ✅ | ✅ | ❌ | ✅ | ❌ |

## Files

```
backend/src/RealSync.Data/Seeders/DataSeeder.cs
```

## Verification

- [x] Seed chạy idempotent (không duplicate khi chạy lại)
- [x] Role Marketing tồn tại sau seed
- [x] Permissions gán đúng theo matrix
