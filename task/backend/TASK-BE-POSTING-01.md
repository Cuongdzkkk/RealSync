# TASK-BE-POSTING-01: Thiết kế Entity Posting

**Module**: Posting Management
**Layer**: Backend — RealSync.Core
**Assignee**: Cường
**Priority**: P0

---

## Mục tiêu

Tạo các entity classes, enums cho module Posting.

## Checklist

- [x] Post entity (kế thừa BaseEntity)
- [x] PostChannel entity
- [x] PostAnalytics entity
- [x] PostSchedule entity
- [x] AIContentGeneration entity
- [x] PostStatus enum (Draft, Scheduled, Published, Failed, Archived)
- [x] PostChannelType enum (Website, Facebook, Batdongsan, Chotot, Zalo)
- [x] PublishStatus enum (Pending, Publishing, Published, Failed)
- [x] ScheduleStatus enum (Pending, Executing, Completed, Failed, Cancelled)
- [x] Navigation properties trên User và Property

## Files

```
backend/src/RealSync.Core/Entities/Post.cs
backend/src/RealSync.Core/Entities/PostChannel.cs
backend/src/RealSync.Core/Entities/PostAnalytics.cs
backend/src/RealSync.Core/Entities/PostSchedule.cs
backend/src/RealSync.Core/Entities/AIContentGeneration.cs
backend/src/RealSync.Core/Enums/PostStatus.cs
backend/src/RealSync.Core/Enums/PostChannelType.cs
backend/src/RealSync.Core/Enums/PublishStatus.cs
backend/src/RealSync.Core/Enums/ScheduleStatus.cs
```

## Verification

- [x] `dotnet build` thành công
- [x] Tất cả entities kế thừa BaseEntity
- [x] Navigation properties đúng chiều
