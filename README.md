# 🏠 RealSync — Real Estate Data & Content Operating System

> Hệ thống vận hành dữ liệu & nội dung bất động sản nội bộ.

[![Backend](https://img.shields.io/badge/Backend-ASP.NET%20Core%208-512BD4?style=flat-square&logo=dotnet)](./backend/)
[![Frontend](https://img.shields.io/badge/Frontend-Vue%203%20+%20Vite-42b883?style=flat-square&logo=vuedotjs)](./frontend/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver)](./docs/)
[![AI](https://img.shields.io/badge/AI-Python%20+%20Node.js-FF6F00?style=flat-square&logo=openai)](./ai/)

---

## 📋 Mục lục

- [Tổng quan](#-tổng-quan)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Bắt đầu nhanh](#-bắt-đầu-nhanh)
- [Modules chính](#-modules-chính)
- [Team & Workflow](#-team--workflow)

---

## 🎯 Tổng quan

**RealSync** là hệ thống quản lý vận hành BĐS nội bộ, tập trung vào:

| Module | Mô tả |
|--------|--------|
| **Data Engine** | Thu thập, chuẩn hóa, lưu trữ dữ liệu BĐS |
| **Product Management** | Quản lý sản phẩm, dự án, listing |
| **Lead Management** | Quản lý khách hàng tiềm năng, pipeline |
| **Crawler Engine** | Thu thập dữ liệu tự động từ các nguồn |
| **AI Classification** | Phân loại, gợi ý, scoring tự động |
| **Content AI Engine** | Sinh nội dung BĐS bằng AI |
| **Dashboard & Insight** | Báo cáo, phân tích, KPI |
| **DevOps/Deploy** | CI/CD, monitoring, infrastructure |

---

## 🏗 Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────┐
│                    Frontend (Vue 3 + Vite)               │
│            Element Plus · Pinia · Axios · SignalR        │
├─────────────────────────────────────────────────────────┤
│                   API Gateway / Reverse Proxy            │
├──────────┬──────────┬──────────┬────────────────────────┤
│  Web API │ SignalR  │ Hangfire │   Background Jobs      │
│ ASP.NET  │  Hub     │  Server  │                        │
├──────────┴──────────┴──────────┴────────────────────────┤
│              Business Logic / Services                   │
├──────────┬──────────┬──────────┬────────────────────────┤
│  Data    │ Product  │  Lead    │  Content AI            │
│  Engine  │  Mgmt    │  Mgmt   │  Engine                │
├──────────┴──────────┴──────────┴────────────────────────┤
│           EF Core + SQL Server + Redis Cache             │
├─────────────────────────────────────────────────────────┤
│        Crawler Engine        │     AI Classification     │
│     Playwright + Puppeteer   │   Python / Node.js        │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Cấu trúc dự án

```
RealSync/
├── backend/                    # ASP.NET Core Web API
│   ├── src/
│   │   ├── RealSync.Api/       # Controllers, Middlewares, Hubs
│   │   ├── RealSync.Core/      # Domain Models, Interfaces
│   │   ├── RealSync.Services/  # Business Logic
│   │   ├── RealSync.Data/      # EF Core, Migrations, Repositories
│   │   └── RealSync.Shared/    # DTOs, Constants, Helpers
│   └── tests/
│       ├── RealSync.UnitTests/
│       └── RealSync.IntegrationTests/
│
├── frontend/                   # Vue 3 + Vite + Element Plus
│   ├── src/
│   │   ├── views/
│   │   ├── components/
│   │   ├── stores/
│   │   ├── composables/
│   │   ├── services/
│   │   ├── router/
│   │   └── assets/
│   └── tests/
│
├── crawler/                    # Crawler Engine
│   ├── src/
│   ├── configs/
│   └── scripts/
│
├── ai/                         # AI Classification + Content AI
│   ├── classification/
│   ├── content-engine/
│   ├── models/
│   └── scripts/
│
├── docs/                       # Tài liệu dự án
│   ├── architecture/
│   ├── api/
│   ├── database/
│   └── guides/
│
├── devops/                     # CI/CD, Docker, Infrastructure
│   ├── docker/
│   ├── nginx/
│   ├── scripts/
│   └── monitoring/
│
├── agent-skills/               # 🧠 Agentic Skill cho AI Assistant
│   ├── SKILL.md
│   ├── scripts/
│   ├── references/
│   ├── examples/
│   └── assets/
│
├── scripts/                    # Project-level scripts
├── .github/                    # GitHub Actions
├── .gitignore
├── docker-compose.yml
└── README.md
```

---

## ⚙ Yêu cầu hệ thống

| Thành phần | Phiên bản tối thiểu |
|-----------|---------------------|
| .NET SDK | 8.0+ |
| Node.js | 20 LTS+ |
| SQL Server | 2019+ |
| Python | 3.11+ |
| Docker | 24.0+ |
| Git | 2.40+ |

---

## 🚀 Bắt đầu nhanh

```bash
# 1. Clone repo
git clone https://github.com/your-org/RealSync.git
cd RealSync

# 2. Backend
cd backend/src/RealSync.Api
dotnet restore
dotnet run

# 3. Frontend
cd frontend
npm install
npm run dev

# 4. Crawler (optional)
cd crawler
npm install
node src/index.js

# 5. Docker (full stack)
docker-compose up -d
```

---

## 👥 Team & Workflow

- **Team size**: 5 người
- **Methodology**: Scrum (Sprint 2 tuần)
- **Branching**: Git Flow (`main` → `develop` → `feature/*`)
- **Code Review**: Bắt buộc trước khi merge
- **CI/CD**: GitHub Actions → Docker → VPS/Cloud

---

## 📄 License

Private & Internal Use Only.

---

> **Maintained by RealSync Team** — Built with ❤️ for Real Estate Operations