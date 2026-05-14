# PowerShell script: Setup Development Environment for RealSync
# Usage: .\setup-dev.ps1

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  RealSync — Development Setup Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# 1. CHECK PREREQUISITES
# ============================================================
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$checks = @(
    @{ Name = ".NET SDK 8.0+"; Command = "dotnet --version"; MinVersion = "8.0" },
    @{ Name = "Node.js 20+"; Command = "node --version"; MinVersion = "v20" },
    @{ Name = "npm"; Command = "npm --version"; MinVersion = "10" },
    @{ Name = "Docker"; Command = "docker --version"; MinVersion = "" },
    @{ Name = "Git"; Command = "git --version"; MinVersion = "" }
)

$allPassed = $true
foreach ($check in $checks) {
    try {
        $version = Invoke-Expression $check.Command 2>&1
        Write-Host "  ✅ $($check.Name): $version" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ $($check.Name): NOT FOUND" -ForegroundColor Red
        $allPassed = $false
    }
}

if (-not $allPassed) {
    Write-Host ""
    Write-Host "⚠ Please install missing prerequisites and try again." -ForegroundColor Red
    exit 1
}

# ============================================================
# 2. SETUP DATABASE (Docker)
# ============================================================
Write-Host ""
Write-Host "🗄 Setting up SQL Server via Docker..." -ForegroundColor Yellow

$dbRunning = docker ps --filter "name=realsync-db" --format "{{.Names}}" 2>$null
if ($dbRunning -eq "realsync-db") {
    Write-Host "  ✅ SQL Server container already running" -ForegroundColor Green
} else {
    docker run -d `
        --name realsync-db `
        -e "ACCEPT_EULA=Y" `
        -e "MSSQL_SA_PASSWORD=RealSync@Dev2026!" `
        -e "MSSQL_PID=Developer" `
        -p 1433:1433 `
        mcr.microsoft.com/mssql/server:2022-latest

    Write-Host "  ⏳ Waiting for SQL Server to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    Write-Host "  ✅ SQL Server started" -ForegroundColor Green
}

# ============================================================
# 3. SETUP BACKEND
# ============================================================
Write-Host ""
Write-Host "🔧 Setting up Backend..." -ForegroundColor Yellow

if (Test-Path "./backend/src/RealSync.Api") {
    Push-Location "./backend"
    
    Write-Host "  📦 Restoring NuGet packages..." -ForegroundColor Gray
    dotnet restore
    
    Write-Host "  🔐 Setting up User Secrets..." -ForegroundColor Gray
    Push-Location "src/RealSync.Api"
    dotnet user-secrets init 2>$null
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=RealSyncDb;User=sa;Password=RealSync@Dev2026!;TrustServerCertificate=true"
    dotnet user-secrets set "Jwt:Secret" "RealSync-Dev-Secret-Key-2026-Must-Be-32-Chars!"
    dotnet user-secrets set "Jwt:Issuer" "RealSync-Dev"
    dotnet user-secrets set "Jwt:Audience" "RealSync-Client"
    Pop-Location
    
    Write-Host "  🗃 Running database migrations..." -ForegroundColor Gray
    dotnet ef database update -p src/RealSync.Data -s src/RealSync.Api 2>$null
    
    Pop-Location
    Write-Host "  ✅ Backend setup complete" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Backend project not found (./backend/src/RealSync.Api)" -ForegroundColor Yellow
    Write-Host "  ℹ Skipping backend setup — will be created later" -ForegroundColor Gray
}

# ============================================================
# 4. SETUP FRONTEND
# ============================================================
Write-Host ""
Write-Host "🎨 Setting up Frontend..." -ForegroundColor Yellow

if (Test-Path "./frontend/package.json") {
    Push-Location "./frontend"
    
    Write-Host "  📦 Installing npm packages..." -ForegroundColor Gray
    npm install
    
    # Create .env.local if not exists
    if (-not (Test-Path ".env.local")) {
        @"
VITE_API_BASE_URL=http://localhost:5000/api/v1
VITE_SIGNALR_URL=http://localhost:5000/hubs
VITE_APP_TITLE=RealSync
"@ | Out-File -FilePath ".env.local" -Encoding utf8
        Write-Host "  📝 Created .env.local" -ForegroundColor Gray
    }
    
    Pop-Location
    Write-Host "  ✅ Frontend setup complete" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Frontend project not found (./frontend/package.json)" -ForegroundColor Yellow
    Write-Host "  ℹ Skipping frontend setup — will be created later" -ForegroundColor Gray
}

# ============================================================
# 5. SETUP CRAWLER (Optional)
# ============================================================
Write-Host ""
Write-Host "🕷 Setting up Crawler..." -ForegroundColor Yellow

if (Test-Path "./crawler/package.json") {
    Push-Location "./crawler"
    npm install
    npx playwright install chromium
    Pop-Location
    Write-Host "  ✅ Crawler setup complete" -ForegroundColor Green
} else {
    Write-Host "  ℹ Crawler project not found — skipping" -ForegroundColor Gray
}

# ============================================================
# 6. SUMMARY
# ============================================================
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  ✅ RealSync Development Setup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "  To start development:" -ForegroundColor Cyan
Write-Host "  Backend:  cd backend/src/RealSync.Api && dotnet run" -ForegroundColor White
Write-Host "  Frontend: cd frontend && npm run dev" -ForegroundColor White
Write-Host "  Crawler:  cd crawler && node src/index.js" -ForegroundColor White
Write-Host ""
Write-Host "  URLs:" -ForegroundColor Cyan
Write-Host "  Backend API:  https://localhost:5001" -ForegroundColor White
Write-Host "  Swagger:      https://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Frontend:     http://localhost:5173" -ForegroundColor White
Write-Host "  SQL Server:   localhost:1433" -ForegroundColor White
Write-Host ""
