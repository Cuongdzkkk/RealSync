# 🚀 Deploy Prompt Template

> Dùng prompt này khi cần agent hỗ trợ setup hoặc deploy.

---

## Prompt Template

```
[DEVOPS] {TaskType}: {Description}

TaskTypes:
- SETUP: Setup môi trường development
- DOCKER: Tạo/sửa Docker configuration
- CI/CD: Tạo/sửa CI/CD pipeline
- DEPLOY: Deploy lên staging/production
- MONITOR: Setup monitoring/logging
- BACKUP: Setup backup strategy

Tuân thủ DevOps Rules trong SKILL.md
```

---

## Ví dụ 1: Setup Development Environment

```
[DEVOPS] SETUP: Setup môi trường development cho team mới

Yêu cầu:
1. Script setup tự động cho Windows/Mac:
   - Check prerequisites (dotnet, node, docker)
   - Clone repo
   - Setup Backend (dotnet restore, user-secrets)
   - Setup Frontend (npm install)
   - Setup Database (docker-compose up sqlserver)
   - Run migrations
   - Seed data
   - Start cả BE + FE

2. Output:
   - scripts/setup-dev.ps1 (Windows)
   - scripts/setup-dev.sh (Mac/Linux)
   - Hướng dẫn trong README
```

---

## Ví dụ 2: Docker Production Setup

```
[DEVOPS] DOCKER: Setup Docker Compose cho production

Yêu cầu:
1. docker-compose.prod.yml
   - Backend API container
   - SQL Server container
   - Redis container
   - Nginx reverse proxy
   - SSL (Let's Encrypt)
   
2. .env.production template

3. Deploy script:
   - Pull latest code
   - Build images
   - Run migrations
   - Zero-downtime deploy
   - Health check
   - Rollback nếu fail
```

---

## Ví dụ 3: CI/CD Pipeline

```
[DEVOPS] CI/CD: Setup GitHub Actions pipeline

Pipeline:
1. On PR → develop:
   - Lint (Backend + Frontend)
   - Unit tests
   - Build check
   
2. On push → develop:
   - Full test suite
   - Build Docker images
   - Deploy to staging
   - Notify team (Slack/Teams)

3. On push → main:
   - Full test suite
   - Build production images
   - Deploy to production (manual approve)
   - Database migration
   - Health check
   - Notify team
```

---

## Deploy Script Example

```powershell
# scripts/deploy.ps1
param(
    [Parameter(Mandatory)]
    [ValidateSet("staging", "production")]
    [string]$Environment
)

Write-Host "🚀 Deploying RealSync to $Environment..." -ForegroundColor Cyan

# 1. Validate
Write-Host "📋 Validating..." -ForegroundColor Yellow
if ($Environment -eq "production") {
    $confirm = Read-Host "Are you sure you want to deploy to PRODUCTION? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "❌ Deployment cancelled" -ForegroundColor Red
        exit 1
    }
}

# 2. Build
Write-Host "🔨 Building..." -ForegroundColor Yellow
docker-compose -f "docker-compose.$Environment.yml" build --no-cache

# 3. Database backup (production only)
if ($Environment -eq "production") {
    Write-Host "💾 Backing up database..." -ForegroundColor Yellow
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    # Add backup command here
}

# 4. Deploy
Write-Host "🚢 Deploying containers..." -ForegroundColor Yellow
docker-compose -f "docker-compose.$Environment.yml" up -d

# 5. Health check
Write-Host "🏥 Running health checks..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

$healthUrl = if ($Environment -eq "staging") { 
    "http://localhost:5000/health" 
} else { 
    "https://api.realsync.vn/health" 
}

try {
    $response = Invoke-RestMethod -Uri $healthUrl -TimeoutSec 10
    if ($response.status -eq "Healthy") {
        Write-Host "✅ Deployment successful!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Health check returned: $($response.status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Health check failed! Consider rollback." -ForegroundColor Red
    exit 1
}

Write-Host "🎉 Done! $Environment deployment complete." -ForegroundColor Green
```
