# TASK-BE-POSTING-02: Migration Database

**Module**: Posting Management
**Layer**: Backend — RealSync.Data
**Assignee**: Cường
**Priority**: P0
**Depends on**: TASK-BE-POSTING-01

---

## Mục tiêu

Tạo EF Core migration cho 5 bảng mới của module Posting.

## Checklist

- [x] PostConfiguration.cs (Fluent API)
- [x] PostChannelConfiguration.cs
- [x] PostAnalyticsConfiguration.cs
- [x] PostScheduleConfiguration.cs
- [x] AIContentGenerationConfiguration.cs
- [x] Thêm 5 DbSet vào RealSyncDbContext
- [x] Tạo migration: `AddPostingModule`
- [x] Apply migration thành công

## Tables

| Table | Quan hệ |
|-------|---------|
| Posts | FK → Properties (nullable), FK → Users |
| PostChannels | FK → Posts (cascade) |
| PostAnalytics | FK → Posts (1-1, cascade) |
| PostSchedules | FK → Posts (cascade) |
| AIContentGenerations | FK → Posts (cascade) |

## Commands

```bash
dotnet ef migrations add AddPostingModule -p RealSync.Data -s RealSync.Api
dotnet ef database update -p RealSync.Data -s RealSync.Api
```

## Verification

- [x] Migration tạo thành công
- [x] Database update không lỗi
- [x] 5 bảng mới xuất hiện trong SQL Server
- [x] FK constraints và indexes đúng
