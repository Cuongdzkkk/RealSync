# 🏗 Architecture Guide — RealSync

> Tài liệu kiến trúc tổng thể cho hệ thống RealSync.

---

## 1. System Architecture Overview

```
                    ┌──────────────────────┐
                    │    Load Balancer      │
                    │    (Nginx / Traefik)  │
                    └──────────┬───────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
    ┌─────────▼──────┐  ┌─────▼──────┐  ┌──────▼─────┐
    │   Frontend     │  │  Backend   │  │   Crawler   │
    │   Vue 3 SPA    │  │  ASP.NET   │  │  Node.js    │
    │   (Static)     │  │  Core API  │  │  Playwright │
    └────────────────┘  └─────┬──────┘  └──────┬──────┘
                              │                │
              ┌───────────────┼────────────────┤
              │               │                │
    ┌─────────▼──────┐  ┌─────▼──────┐  ┌──────▼─────┐
    │   SQL Server   │  │   Redis    │  │ AI Service  │
    │   (Primary DB) │  │  (Cache)   │  │ Python API  │
    └────────────────┘  └────────────┘  └─────────────┘
```

## 2. Backend Architecture (Clean Architecture Light)

### Layer Diagram
```
┌─────────────────────────────────────────┐
│           RealSync.Api                  │  ← Presentation Layer
│  Controllers, Middlewares, Hubs, Filters │
├─────────────────────────────────────────┤
│         RealSync.Services               │  ← Application Layer
│  Service Implementations, Validators    │
│  AutoMapper Profiles, Background Jobs   │
├─────────────────────────────────────────┤
│           RealSync.Core                 │  ← Domain Layer
│  Entities, Interfaces, Enums, Events    │
├─────────────────────────────────────────┤
│           RealSync.Data                 │  ← Infrastructure Layer
│  DbContext, Repositories, Migrations    │
├─────────────────────────────────────────┤
│          RealSync.Shared                │  ← Cross-cutting
│  DTOs, Constants, Helpers, Exceptions   │
└─────────────────────────────────────────┘
```

### Dependency Flow
```
Api → Services → Core ← Data
         ↓
       Shared (cross-cutting)
```

**Quy tắc quan trọng:**
- `Core` KHÔNG phụ thuộc vào bất kỳ layer nào khác.
- `Data` implement interfaces từ `Core`.
- `Services` implement interfaces từ `Core`, sử dụng `Data` qua DI.
- `Api` chỉ phụ thuộc vào `Services` và `Shared`.

## 3. Frontend Architecture

### Component Hierarchy
```
App.vue
├── AppLayout.vue
│   ├── AppHeader.vue
│   ├── AppSidebar.vue
│   └── AppContent.vue (router-view)
│       ├── DashboardView.vue
│       ├── PropertyListView.vue
│       │   ├── PropertyTable.vue
│       │   ├── PropertyFilter.vue
│       │   └── PropertyCard.vue
│       ├── PropertyDetailView.vue
│       │   ├── PropertyInfo.vue
│       │   ├── PropertyImages.vue
│       │   └── PropertyMap.vue
│       ├── LeadListView.vue
│       └── LeadDetailView.vue
└── AuthLayout.vue
    ├── LoginView.vue
    └── ForgotPasswordView.vue
```

### Data Flow
```
Component → Action (Pinia) → API Service (Axios) → Backend
    ↑                                                  │
    └──────────── State (Pinia Store) ←────────────────┘
```

### SignalR Real-time Flow
```
Backend Event → SignalR Hub → Frontend SignalR Client → Pinia Store → Component Update
```

## 4. Database Architecture

### Schema Groups
```
[dbo]
├── Core Tables
│   ├── Properties
│   ├── PropertyTypes
│   ├── PropertyImages
│   ├── Projects
│   └── Areas
│
├── Lead Tables
│   ├── Leads
│   ├── LeadActivities
│   ├── LeadSources
│   └── LeadStatuses
│
├── User Tables
│   ├── Users
│   ├── Roles
│   └── UserRoles
│
├── Crawler Tables
│   ├── CrawlSources
│   ├── CrawlJobs
│   └── CrawlResults
│
└── System Tables
    ├── AuditLogs
    ├── AppSettings
    └── Notifications
```

## 5. API Architecture

### API Versioning
```
/api/v1/properties          ← Current version
/api/v2/properties          ← Future version (khi cần breaking changes)
```

### Endpoint Pattern
```
GET    /api/v1/{resource}          → List (with pagination)
GET    /api/v1/{resource}/{id}     → Get by ID
POST   /api/v1/{resource}          → Create
PUT    /api/v1/{resource}/{id}     → Update (full)
PATCH  /api/v1/{resource}/{id}     → Update (partial)
DELETE /api/v1/{resource}/{id}     → Soft delete
```

## 6. Deployment Architecture

### Production Setup
```
┌─────────────────────────────────────┐
│           VPS / Cloud Server        │
│                                     │
│  ┌─────────────────────────────┐    │
│  │        Nginx                │    │
│  │  - SSL Termination          │    │
│  │  - Static files (Frontend)  │    │
│  │  - Reverse proxy (Backend)  │    │
│  └──────────┬──────────────────┘    │
│             │                       │
│  ┌──────────▼──────────────────┐    │
│  │   Docker Compose            │    │
│  │  ┌────────┐  ┌───────────┐  │    │
│  │  │ API    │  │ SQL Server│  │    │
│  │  │ (.NET) │  │           │  │    │
│  │  └────────┘  └───────────┘  │    │
│  │  ┌────────┐  ┌───────────┐  │    │
│  │  │Hangfire│  │  Redis    │  │    │
│  │  │Worker  │  │  (Cache)  │  │    │
│  │  └────────┘  └───────────┘  │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

## 7. Security Architecture

```
Client Request
    │
    ▼
┌──────────────┐
│ Rate Limiter │ → Block if exceeded
├──────────────┤
│ CORS Check   │ → Block if origin not allowed
├──────────────┤
│ JWT Auth     │ → 401 if invalid token
├──────────────┤
│ Role Check   │ → 403 if insufficient permissions
├──────────────┤
│ Validation   │ → 400 if invalid input
├──────────────┤
│ Controller   │ → Process request
├──────────────┤
│ Service      │ → Business logic + audit log
├──────────────┤
│ Repository   │ → Database query
└──────────────┘
```

---

> Tham khảo thêm: `database-guide.md`, `api-guide.md`, `deployment-guide.md`
