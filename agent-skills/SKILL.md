# 🧠 RealSync — Agentic Skill Definition

> Đây là bộ não chính của AI Agent khi làm việc với dự án RealSync.
> Agent PHẢI đọc file này trước khi thực hiện bất kỳ task nào.

---

## 1. 📋 Project Overview

| Field | Value |
|-------|-------|
| **Tên dự án** | RealSync — Real Estate Data & Content Operating System |
| **Loại** | Hệ thống BĐS nội bộ |
| **Team** | 5 người, làm việc theo Scrum |
| **Sprint** | 2 tuần |
| **Ngôn ngữ chính** | C# (Backend), TypeScript/JavaScript (Frontend), Python (AI) |
| **MVP Focus** | Data Engine, Product Management, Lead Management |

### Mục tiêu dự án
- Thu thập và chuẩn hóa dữ liệu BĐS từ nhiều nguồn
- Quản lý sản phẩm, dự án, listing BĐS
- Quản lý khách hàng tiềm năng (Lead pipeline)
- Tự động hóa phân loại và tạo nội dung bằng AI
- Cung cấp dashboard & insight cho team vận hành

---

## 2. 🛠 Tech Stack

### Backend
| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | ASP.NET Core Web API | 8.0+ |
| ORM | Entity Framework Core | 8.0+ |
| Database | SQL Server | 2019+ |
| Real-time | SignalR | Built-in |
| Background Jobs | Hangfire | 1.8+ |
| Caching | Redis (optional) | 7.0+ |
| Authentication | JWT Bearer | - |
| API Docs | Swagger / Swashbuckle | - |

### Frontend
| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | Vue 3 | 3.4+ |
| Build Tool | Vite | 5.0+ |
| UI Library | Element Plus | 2.5+ |
| State | Pinia | 2.1+ |
| HTTP Client | Axios | 1.6+ |
| Router | Vue Router | 4.2+ |
| Real-time | @microsoft/signalr | 8.0+ |
| Charts | ECharts / vue-echarts | 5.5+ |

### Crawler Engine
| Component | Technology |
|-----------|-----------|
| Browser Automation | Playwright (Node.js) |
| HTTP Scraping | Cheerio, Axios |
| Scheduling | Hangfire (Backend) hoặc Cron |
| Anti-detect | Proxy rotation, stealth plugin |

### AI Module
| Component | Technology |
|-----------|-----------|
| Classification | Python (scikit-learn / fastapi) |
| Content Generation | OpenAI API / Local LLM |
| NLP | spaCy, underthesea (Vietnamese) |
| Orchestration | Node.js hoặc Python FastAPI |

### DevOps
| Component | Technology |
|-----------|-----------|
| Container | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Reverse Proxy | Nginx |
| Monitoring | Serilog + Seq / Grafana |
| SSL | Let's Encrypt / Certbot |

---

## 3. 📦 Main Modules

### Module Map
```
RealSync/
├── [M1] Data Engine          → Thu thập, chuẩn hóa, lưu trữ dữ liệu
├── [M2] Product Management   → CRUD sản phẩm, dự án, listing
├── [M3] Lead Management      → Quản lý khách hàng, pipeline, follow-up
├── [M4] Crawler Engine       → Auto crawl từ các trang BĐS
├── [M5] AI Classification    → Phân loại tự động (loại BĐS, khu vực, giá)
├── [M6] Content AI Engine    → Sinh mô tả, SEO content, social post
├── [M7] Dashboard & Insight  → Báo cáo, KPI, phân tích thị trường
├── [M8] DevOps/Deploy        → CI/CD, monitoring, infrastructure
└── [M9] Posting Management   → Quản lý đăng bài BĐS đa kênh
```

### Module Priority (MVP)
| Priority | Module | Status |
|----------|--------|--------|
| 🔴 P0 | M1 Data Engine | MVP |
| 🔴 P0 | M2 Product Management | MVP |
| 🔴 P0 | M3 Lead Management | MVP |
| 🔴 P0 | M9 Posting Management | MVP |
| 🟡 P1 | M4 Crawler Engine | Phase 2 |
| 🟡 P1 | M5 AI Classification | Phase 2 |
| 🟢 P2 | M6 Content AI Engine | Phase 3 |
| 🟢 P2 | M7 Dashboard & Insight | Phase 3 |
| 🔵 Ongoing | M8 DevOps/Deploy | Continuous |

---

## 4. 📐 Backend Rules

### Architecture Pattern
```
Controller → Service → Repository → Database
     ↓           ↓          ↓
   DTO      Interface    EF Core
```

### Project Structure
```
backend/src/
├── RealSync.Api/
│   ├── Controllers/         # API Controllers (thin, chỉ gọi service)
│   ├── Middlewares/          # Error handling, logging, auth
│   ├── Hubs/                # SignalR Hubs
│   ├── Filters/             # Action/Exception filters
│   ├── Extensions/          # Service registration extensions
│   └── Program.cs           # Entry point
│
├── RealSync.Core/
│   ├── Entities/            # Domain entities (EF models)
│   ├── Interfaces/          # Service & Repository interfaces
│   ├── Enums/               # Enums
│   └── Events/              # Domain events
│
├── RealSync.Services/
│   ├── Implementations/     # Service implementations
│   ├── Validators/          # FluentValidation validators
│   ├── Mappings/            # AutoMapper profiles
│   └── Jobs/                # Hangfire background jobs
│
├── RealSync.Data/
│   ├── Context/             # DbContext
│   ├── Repositories/        # Repository implementations
│   ├── Migrations/          # EF Migrations
│   ├── Configurations/      # Entity configurations (Fluent API)
│   └── Seeders/             # Data seeders
│
└── RealSync.Shared/
    ├── DTOs/                # Data Transfer Objects
    │   ├── Requests/        # Input DTOs
    │   └── Responses/       # Output DTOs
    ├── Constants/           # App constants
    ├── Helpers/             # Utility classes
    └── Exceptions/          # Custom exceptions
```

### Backend Coding Rules
1. **Controller phải thin** — Chỉ validate input, gọi service, trả response.
2. **Business logic ở Service layer** — Không viết logic ở Controller hay Repository.
3. **Dùng Interface** — Mọi Service/Repository phải có Interface.
4. **Dependency Injection** — Đăng ký qua Extension methods.
5. **Async/Await** — Tất cả I/O operations phải async.
6. **Response chuẩn** — Dùng `ApiResponse<T>` wrapper thống nhất.
7. **Validation** — Dùng FluentValidation, không validate manual.
8. **Logging** — Dùng Serilog, structured logging.
9. **Pagination** — Tất cả list API phải support pagination.
10. **Soft Delete** — Entity dùng `IsDeleted` flag, không hard delete.

### API Response Format
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy danh sách thành công",
  "data": { },
  "errors": null,
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

### Naming Convention (Backend)
| Element | Convention | Example |
|---------|-----------|---------|
| Class | PascalCase | `PropertyService` |
| Interface | I + PascalCase | `IPropertyService` |
| Method | PascalCase + Async | `GetPropertiesAsync()` |
| Variable | camelCase | `propertyList` |
| Constant | UPPER_SNAKE | `MAX_PAGE_SIZE` |
| DTO | PascalCase + Suffix | `PropertyCreateRequest` |
| Endpoint | kebab-case | `/api/v1/properties` |

---

## 5. 🎨 Frontend Rules

### Project Structure
```
frontend/src/
├── views/                   # Page components (1 file per route)
│   ├── dashboard/
│   ├── properties/
│   ├── leads/
│   ├── crawlers/
│   └── settings/
│
├── components/              # Reusable components
│   ├── common/              # Button, Modal, Table, Form...
│   ├── layout/              # AppHeader, AppSidebar, AppFooter
│   ├── property/            # Property-specific components
│   ├── lead/                # Lead-specific components
│   └── charts/              # Chart components
│
├── stores/                  # Pinia stores
│   ├── useAuthStore.ts
│   ├── usePropertyStore.ts
│   ├── useLeadStore.ts
│   └── useAppStore.ts
│
├── composables/             # Vue composables (reusable logic)
│   ├── useApi.ts
│   ├── usePagination.ts
│   ├── useSignalR.ts
│   └── usePermission.ts
│
├── services/                # API service layer
│   ├── api.ts               # Axios instance & interceptors
│   ├── propertyService.ts
│   ├── leadService.ts
│   └── authService.ts
│
├── router/                  # Vue Router
│   ├── index.ts
│   └── guards.ts
│
├── types/                   # TypeScript types/interfaces
│   ├── property.ts
│   ├── lead.ts
│   └── common.ts
│
├── utils/                   # Utility functions
│   ├── format.ts
│   ├── validate.ts
│   └── constants.ts
│
├── assets/                  # Static assets
│   ├── styles/
│   ├── images/
│   └── icons/
│
├── App.vue
└── main.ts
```

### Frontend Coding Rules
1. **Composition API** — Chỉ dùng `<script setup>`, không dùng Options API.
2. **TypeScript** — Bắt buộc dùng TypeScript cho mọi file `.ts` và `.vue`.
3. **Pinia** — Quản lý state global, không dùng props drilling quá 2 cấp.
4. **Element Plus** — Ưu tiên dùng component có sẵn, không tự build.
5. **Axios Interceptor** — Xử lý auth token, error handling tập trung.
6. **Composable pattern** — Logic dùng lại phải viết thành composable.
7. **Responsive** — Desktop-first, hỗ trợ tablet (≥768px).
8. **i18n Ready** — Text hiển thị nên hỗ trợ đa ngôn ngữ (Vietnamese mặc định).
9. **Loading states** — Mọi API call phải có loading indicator.
10. **Error handling** — Hiển thị ElMessage/ElNotification khi có lỗi.

### Naming Convention (Frontend)
| Element | Convention | Example |
|---------|-----------|---------|
| Component | PascalCase | `PropertyCard.vue` |
| Composable | camelCase + use | `useProperty.ts` |
| Store | camelCase + use | `usePropertyStore.ts` |
| Service | camelCase + Service | `propertyService.ts` |
| Variable | camelCase | `propertyList` |
| Constant | UPPER_SNAKE | `API_BASE_URL` |
| CSS class | kebab-case / BEM | `property-card__title` |
| Event | camelCase | `@updateProperty` |
| Route | kebab-case | `/properties/:id` |

---

## 6. 🗄 Database Rules

### General Rules
1. **Naming**: Tên bảng PascalCase số nhiều (`Properties`, `Leads`).
2. **Primary Key**: Dùng `GUID` (`uniqueidentifier`) cho PK.
3. **Audit Fields**: Mọi bảng phải có `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`.
4. **Soft Delete**: Dùng `IsDeleted` (bit) + `DeletedAt` (datetime2).
5. **Index**: Tạo index cho cột thường query (FK, status, created_at).
6. **Migration**: Mỗi thay đổi schema phải tạo migration riêng.
7. **Seed Data**: Data mẫu phải trong Seeder, không insert thủ công.
8. **No Raw SQL**: Ưu tiên LINQ, chỉ dùng raw SQL khi cần performance.

### Core Tables (MVP)
```
Properties          → Bất động sản
PropertyTypes       → Loại BĐS (đất, nhà, căn hộ...)
PropertyImages      → Hình ảnh BĐS
Projects            → Dự án BĐS
Areas               → Khu vực (tỉnh, quận, phường)
Leads               → Khách hàng tiềm năng
LeadActivities      → Lịch sử tương tác
Users               → Người dùng hệ thống
Roles               → Vai trò
CrawlSources        → Nguồn crawl
CrawlJobs           → Job crawl
CrawlResults        → Kết quả crawl
```

### Entity Base Class
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

---

## 7. 🤖 AI Rules

### General
1. **API-first** — AI module expose REST API, Backend gọi qua HTTP.
2. **Async processing** — AI tasks nặng chạy background (Hangfire queue).
3. **Fallback** — Nếu AI service down, hệ thống vẫn hoạt động bình thường.
4. **Rate limiting** — Giới hạn số lượng AI request/phút.
5. **Cost tracking** — Log token usage cho mỗi AI call.

### AI Classification Rules
- Input: Raw property data (text, images)
- Output: Classified category, confidence score, extracted features
- Model: Ưu tiên rule-based + ML, sau đó mới dùng LLM
- Accuracy target: ≥ 85% cho MVP

### Content AI Rules
- Sinh mô tả BĐS từ structured data
- SEO-optimized content (meta title, description)
- Social media post generation
- Template-based + AI enhancement
- Output phải qua human review trước khi publish

---

## 8. 🕷 Crawler Rules

### General
1. **Respectful crawling** — Tuân thủ robots.txt, rate limit 2-5s/request.
2. **Proxy rotation** — Dùng proxy pool, rotate mỗi 10-20 requests.
3. **Error resilience** — Retry 3 lần, exponential backoff.
4. **Data validation** — Validate data sau khi crawl trước khi lưu DB.
5. **Deduplication** — Check trùng lặp bằng URL + content hash.
6. **Scheduling** — Chạy theo schedule, không crawl liên tục.
7. **Logging** — Log mọi crawl session (source, count, errors, duration).

### Crawler Architecture
```
Scheduler → Job Queue → Crawler Worker → Parser → Validator → Database
                ↓
         Anti-detect Layer (proxy, headers, fingerprint)
```

### Supported Sources (MVP)
- batdongsan.com.vn
- nhadat24h.net
- chotot.com (BĐS section)
- Custom sources (configurable)

---

## 9. 🚀 DevOps Rules

### Environment
| Env | Purpose | Branch |
|-----|---------|--------|
| `development` | Local dev | `feature/*` |
| `staging` | Testing, QA | `develop` |
| `production` | Live | `main` |

### Docker Rules
1. Multi-stage build cho Backend và Frontend.
2. Dùng `docker-compose.yml` cho local development.
3. `.env` file cho mỗi environment (KHÔNG commit `.env`).
4. Health checks cho mọi container.

### CI/CD Pipeline
```
Push → Lint → Test → Build → Docker Image → Deploy
```

### Monitoring
- Application logs: Serilog → Seq/Elasticsearch
- Performance: Response time, error rate, throughput
- Infrastructure: CPU, Memory, Disk, Network
- Alerts: Slack/Teams notification khi error rate > threshold

---

## 10. 🔒 Security Rules

### Authentication & Authorization
1. **JWT Bearer** — Access token (15 min) + Refresh token (7 days).
2. **Role-based** — Admin, Manager, Agent, Viewer.
3. **API Key** — Cho crawler và AI service internal.
4. **CORS** — Whitelist chỉ frontend domain.

### Data Security
1. **Password** — Hash bằng BCrypt, KHÔNG lưu plaintext.
2. **SQL Injection** — Dùng parameterized queries (EF Core default).
3. **XSS** — Sanitize input, encode output.
4. **HTTPS** — Bắt buộc production.
5. **Secrets** — Dùng User Secrets (dev) / Azure Key Vault (prod).
6. **Rate Limiting** — API rate limit per user/IP.

### Sensitive Data
- KHÔNG log password, token, API key.
- KHÔNG expose internal errors ra client.
- KHÔNG commit secrets, connection strings vào Git.

---

## 11. 📝 Coding Convention

### General
- **DRY** — Không lặp code, extract thành function/service.
- **KISS** — Giữ đơn giản, không over-engineer.
- **SOLID** — Tuân thủ SOLID principles.
- **Comments** — Comment cho logic phức tạp, không comment cho code hiển nhiên.
- **File size** — Một file không quá 300 dòng, tách nếu dài hơn.
- **Function size** — Một function không quá 50 dòng.

### Error Handling
```csharp
// Backend: Custom exception + Global handler
throw new NotFoundException("Property", propertyId);
throw new BusinessException("Sản phẩm đã hết hạn");
throw new ForbiddenException("Không có quyền truy cập");
```

```typescript
// Frontend: Centralized error handling
try {
  const data = await propertyService.getById(id);
} catch (error) {
  ElMessage.error(error.response?.data?.message || 'Có lỗi xảy ra');
}
```

### Code Review Checklist
- [ ] Có unit test cho logic mới?
- [ ] Đã validate input?
- [ ] Có error handling?
- [ ] Naming convention đúng?
- [ ] Không hardcode giá trị?
- [ ] Không lặp code?
- [ ] Performance có ổn?
- [ ] Security có đảm bảo?

---

## 12. 🔀 Git Convention

### Branch Strategy
```
main (production)
 └── develop (staging)
      ├── feature/M2-property-crud
      ├── feature/M3-lead-pipeline
      ├── bugfix/fix-login-redirect
      └── hotfix/patch-api-crash
```

### Commit Message Format
```
<type>(<scope>): <subject>

Types: feat, fix, refactor, docs, style, test, chore, perf
Scope: backend, frontend, crawler, ai, devops, docs

Ví dụ:
feat(backend): add property CRUD endpoints
fix(frontend): resolve pagination issue in lead list
refactor(backend): extract property validation to service
docs(docs): update API documentation
chore(devops): update Docker base image
test(backend): add unit tests for LeadService
```

### PR Rules
1. Tối thiểu 1 reviewer approve.
2. Mọi PR phải pass CI (lint + test).
3. PR description phải có: What, Why, How, Test plan.
4. Squash merge vào develop, rebase merge vào main.
5. Xóa branch sau khi merge.

---

## 13. ⚡ Agent Workflow

### Khi nhận task mới, Agent PHẢI tuân theo workflow sau:

```
┌─────────────────────────────────────────────┐
│  1. ĐỌC SKILL.MD + REFERENCE GUIDES        │
│     → Hiểu architecture, rules, conventions │
├─────────────────────────────────────────────┤
│  2. PHÂN TÍCH TASK                          │
│     → Xác định module, layer, scope         │
│     → Kiểm tra task có trong MVP scope?     │
├─────────────────────────────────────────────┤
│  3. KIỂM TRA CODE HIỆN TẠI                 │
│     → Đọc code liên quan                    │
│     → Hiểu pattern đang dùng               │
├─────────────────────────────────────────────┤
│  4. LÊN KẾ HOẠCH                           │
│     → List các file cần tạo/sửa            │
│     → Xác định dependencies                │
├─────────────────────────────────────────────┤
│  5. IMPLEMENT                               │
│     → Code theo convention                  │
│     → Tạo/sửa từng file                    │
├─────────────────────────────────────────────┤
│  6. VERIFY                                  │
│     → Kiểm tra code chạy đúng              │
│     → Chạy test nếu có                     │
├─────────────────────────────────────────────┤
│  7. BÁO CÁO                                │
│     → Tóm tắt những gì đã làm             │
│     → List file đã tạo/sửa                │
│     → Gợi ý bước tiếp theo                │
└─────────────────────────────────────────────┘
```

### Task Classification
| Tag | Meaning | Action |
|-----|---------|--------|
| `[BACKEND]` | Task Backend | Làm việc trong `backend/` |
| `[FRONTEND]` | Task Frontend | Làm việc trong `frontend/` |
| `[CRAWLER]` | Task Crawler | Làm việc trong `crawler/` |
| `[AI]` | Task AI | Làm việc trong `ai/` |
| `[DEVOPS]` | Task DevOps | Làm việc trong `devops/` |
| `[FULLSTACK]` | Task cả BE + FE | Cả `backend/` + `frontend/` |
| `[DOCS]` | Task tài liệu | Làm việc trong `docs/` |

---

## 14. ✅ Agent Được Phép Làm

1. ✅ Tạo code mới theo đúng architecture và convention.
2. ✅ Refactor code hiện tại để cải thiện chất lượng.
3. ✅ Fix bug khi được yêu cầu.
4. ✅ Tạo migration cho database changes.
5. ✅ Viết unit test và integration test.
6. ✅ Tạo/cập nhật documentation.
7. ✅ Setup Docker, CI/CD configurations.
8. ✅ Tạo script hỗ trợ (setup, deploy, test).
9. ✅ Tối ưu performance khi được yêu cầu.
10. ✅ Gợi ý cải tiến architecture.
11. ✅ Tạo seed data và test data.
12. ✅ Tạo API endpoints theo chuẩn RESTful.

---

## 15. 🚫 Agent KHÔNG Được Phép Làm

1. ❌ **KHÔNG** tự thêm feature ngoài scope MVP mà chưa được approve.
2. ❌ **KHÔNG** thay đổi business direction hoặc core architecture.
3. ❌ **KHÔNG** xóa code/file mà không được yêu cầu.
4. ❌ **KHÔNG** commit trực tiếp vào `main` hoặc `develop` branch.
5. ❌ **KHÔNG** hardcode credentials, API keys, connection strings.
6. ❌ **KHÔNG** bỏ qua error handling hoặc validation.
7. ❌ **KHÔNG** tạo file quá dài (>300 dòng) mà không tách.
8. ❌ **KHÔNG** dùng package/library mới mà chưa được approve.
9. ❌ **KHÔNG** thay đổi database schema mà không tạo migration.
10. ❌ **KHÔNG** bỏ qua security rules (auth, CORS, input validation).
11. ❌ **KHÔNG** viết code "just to make it work" — phải production-ready.
12. ❌ **KHÔNG** ignore existing patterns — follow cái đã có.

---

## 16. 📚 Reference Files

Agent nên đọc các file sau để hiểu sâu hơn:

| File | Mô tả |
|------|-------|
| `references/architecture-guide.md` | Hướng dẫn kiến trúc tổng thể |
| `references/database-guide.md` | Database schema, conventions |
| `references/api-guide.md` | API design guide, endpoints |
| `references/ai-guide.md` | AI module integration guide |
| `references/crawler-guide.md` | Crawler setup, rules |
| `references/deployment-guide.md` | Deployment procedures |
| `crawl4ai/SKILL.md` | Crawl4AI — Crawl JS-heavy sites, chống bot, batch crawl |
| `scrapfly-scraper/SKILL.md` | Scrapfly Scraper — Anti-detect web scraping API |
| `scrapfly-crawler/SKILL.md` | Scrapfly Crawler — Site-wide crawl + real-time progress |
| `scrapfly-browser/SKILL.md` | Scrapfly Cloud Browser — Remote browser automation |
| `crawler-tiered/SKILL.md` | Crawler 3 tầng — Tự động chọn chiến lược crawl |

## 17. 🎯 Quick Decision Tree

```
Nhận task →
  ├── Task có trong MVP scope? 
  │     ├── YES → Tiếp tục
  │     └── NO  → Hỏi lại user, đề xuất đưa vào backlog
  │
  ├── Task thuộc module nào?
  │     → Xác định [BACKEND] / [FRONTEND] / [FULLSTACK] / [CRAWLER] / [AI]
  │
  ├── Đã có code liên quan?
  │     ├── YES → Đọc và follow pattern hiện tại
  │     └── NO  → Tạo mới theo convention trong SKILL.md
  │
  ├── Cần thay đổi DB?
  │     ├── YES → Tạo entity → Configuration → Migration → Seed
  │     └── NO  → Tiếp tục
  │
  └── Implement → Test → Report
```

---

> **Last Updated**: 2026-05-14
> **Version**: 1.0.0
> **Maintained by**: RealSync Team
