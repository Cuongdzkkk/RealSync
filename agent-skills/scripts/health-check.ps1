# PowerShell: Kiểm tra health tất cả services
# Usage: .\health-check.ps1 [-Environment staging|production]

param(
    [ValidateSet("development", "staging", "production")]
    [string]$Environment = "development"
)

$ErrorActionPreference = "SilentlyContinue"

Write-Host ""
Write-Host "🏥 RealSync Health Check — $Environment" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrls = @{
    "development" = "http://localhost:5000"
    "staging"     = "https://api-staging.realsync.vn"
    "production"  = "https://api.realsync.vn"
}

$baseUrl = $baseUrls[$Environment]

# Check Backend API
Write-Host "🔧 Backend API..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -TimeoutSec 5
    Write-Host "  ✅ Status: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Backend API is DOWN" -ForegroundColor Red
}

# Check Database
Write-Host "🗄 Database..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health/ready" -TimeoutSec 5
    Write-Host "  ✅ Database connected" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Database connection failed" -ForegroundColor Red
}

# Check Redis
Write-Host "📦 Redis Cache..." -ForegroundColor Yellow
if ($Environment -eq "development") {
    $redisRunning = docker ps --filter "name=realsync-redis" --format "{{.Status}}" 2>$null
    if ($redisRunning) {
        Write-Host "  ✅ Redis: $redisRunning" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Redis not running (optional)" -ForegroundColor Yellow
    }
}

# Check Frontend
Write-Host "🎨 Frontend..." -ForegroundColor Yellow
$frontendUrls = @{
    "development" = "http://localhost:5173"
    "staging"     = "https://staging.realsync.vn"
    "production"  = "https://realsync.vn"
}
try {
    $frontendResponse = Invoke-WebRequest -Uri $frontendUrls[$Environment] -TimeoutSec 5
    Write-Host "  ✅ Status: $($frontendResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Frontend is DOWN" -ForegroundColor Red
}

Write-Host ""
Write-Host "✅ Health check complete" -ForegroundColor Green
