# PowerShell: Chạy migration database
# Usage: .\run-migration.ps1 -Name "AddPropertyTable"

param(
    [Parameter(Mandatory)]
    [string]$Name,
    
    [switch]$Apply,
    [switch]$Script
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "../../backend")
$DataProject = Join-Path $ProjectRoot "src/RealSync.Data"
$ApiProject = Join-Path $ProjectRoot "src/RealSync.Api"

Write-Host "🗃 RealSync Database Migration" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

if (-not (Test-Path $DataProject)) {
    Write-Host "❌ Backend project not found at: $ProjectRoot" -ForegroundColor Red
    exit 1
}

# Create migration
Write-Host "📝 Creating migration: $Name" -ForegroundColor Yellow
dotnet ef migrations add $Name -p $DataProject -s $ApiProject

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Migration '$Name' created successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to create migration" -ForegroundColor Red
    exit 1
}

# Apply migration
if ($Apply) {
    Write-Host "🚀 Applying migration to database..." -ForegroundColor Yellow
    dotnet ef database update -p $DataProject -s $ApiProject
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migration applied successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to apply migration" -ForegroundColor Red
        exit 1
    }
}

# Generate SQL script
if ($Script) {
    $outputPath = Join-Path $ProjectRoot "migrations/$Name.sql"
    Write-Host "📄 Generating SQL script..." -ForegroundColor Yellow
    dotnet ef migrations script --idempotent -p $DataProject -s $ApiProject -o $outputPath
    Write-Host "✅ Script saved to: $outputPath" -ForegroundColor Green
}
