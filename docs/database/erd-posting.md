# 📊 ERD — Posting Module

> Entity Relationship Diagram cho module Posting Management.

---

## Diagram

```mermaid
erDiagram
    Users ||--o{ Posts : "creates (AuthorId)"
    Properties ||--o{ Posts : "linked to (PropertyId)"
    Posts ||--o{ PostChannels : "has channels"
    Posts ||--o| PostAnalytics : "has analytics (1-1)"
    Posts ||--o{ PostSchedules : "has schedules"
    Posts ||--o{ AIContentGenerations : "has AI content"

    Users {
        uniqueidentifier Id PK
        nvarchar FullName
        nvarchar Email
        uniqueidentifier RoleId FK
    }

    Properties {
        uniqueidentifier Id PK
        nvarchar Title
        nvarchar PropertyCode
        nvarchar Status
    }

    Posts {
        uniqueidentifier Id PK
        nvarchar Title
        nvarchar_max Content
        nvarchar Summary
        nvarchar ThumbnailUrl
        uniqueidentifier PropertyId FK "nullable"
        uniqueidentifier AuthorId FK
        nvarchar Status "Draft, Scheduled, Published, Failed, Archived"
        datetime2 PublishedAt
        datetime2 CreatedAt
        datetime2 UpdatedAt
        bit IsDeleted
    }

    PostChannels {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK
        nvarchar Channel "Website, Facebook, Batdongsan, Chotot, Zalo"
        nvarchar PublishStatus "Pending, Publishing, Published, Failed"
        nvarchar PublishedUrl
        datetime2 PublishedAt
        nvarchar ErrorMessage
        datetime2 CreatedAt
        bit IsDeleted
    }

    PostAnalytics {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK "unique"
        int Views
        int Clicks
        int LeadsGenerated
        decimal ConversionRate
        datetime2 LastUpdated
        datetime2 CreatedAt
        bit IsDeleted
    }

    PostSchedules {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK
        datetime2 ScheduledAt
        nvarchar Status "Pending, Executing, Completed, Failed, Cancelled"
        datetime2 CreatedAt
        bit IsDeleted
    }

    AIContentGenerations {
        uniqueidentifier Id PK
        uniqueidentifier PostId FK
        nvarchar_max Prompt
        nvarchar_max GeneratedContent
        datetime2 CreatedAt
        bit IsDeleted
    }
```

---

## Relationships

| Parent | Child | Type | FK | Delete Behavior |
|--------|-------|------|----|-----------------|
| Users | Posts | 1-N | AuthorId | Restrict |
| Properties | Posts | 1-N | PropertyId (nullable) | SetNull |
| Posts | PostChannels | 1-N | PostId | Cascade |
| Posts | PostAnalytics | 1-1 | PostId (unique) | Cascade |
| Posts | PostSchedules | 1-N | PostId | Cascade |
| Posts | AIContentGenerations | 1-N | PostId | Cascade |

---

## Indexes

| Table | Column(s) | Type |
|-------|-----------|------|
| Posts | Status | Index |
| Posts | AuthorId | Index |
| Posts | PropertyId | Index |
| Posts | PublishedAt | Index |
| Posts | CreatedAt | Index |
| PostChannels | PostId | Index |
| PostChannels | Channel | Index |
| PostChannels | PublishStatus | Index |
| PostAnalytics | PostId | Unique Index |
| PostSchedules | PostId | Index |
| PostSchedules | ScheduledAt | Index |
| PostSchedules | Status | Index |
| AIContentGenerations | PostId | Index |
| AIContentGenerations | CreatedAt | Index |

---

> **Version**: 1.0.0
> **Last Updated**: 2026-06-02
