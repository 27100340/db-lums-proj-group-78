# Test Backend API Connection

Write-Host "===================================" -ForegroundColor Cyan
Write-Host "test backend api" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"

Write-Host "test api: http://localhost:5000/api/stats/counts" -ForegroundColor Yellow
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/stats/counts" -Method Get
    Write-Host " backend  connected to kubernetes sql server YAyyyyy" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database Statistics:" -ForegroundColor Cyan
    $response | Format-List
    Write-Host ""
} catch {
    Write-Host "problem: connect to backend failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "error info:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host ""
    Write-Host "ensure:" -ForegroundColor Yellow
    Write-Host "  1. backend is running by run start-backend.ps1" -ForegroundColor White
    Write-Host "  2. backend should shows 'Database: ServiceConnect on localhost,31433'" -ForegroundColor White
    Write-Host "  3. make sure firewall not blocking port 5000" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "testing another endpt: http://localhost:5000/api/servicecategories" -ForegroundColor Yellow
Write-Host ""

try {
    $categories = Invoke-RestMethod -Uri "http://localhost:5000/api/servicecategories" -Method Get
    Write-Host "ervice categories fetched" -ForegroundColor Green
    Write-Host ""
    Write-Host "first 3 categories:" -ForegroundColor Cyan
    $categories | Select-Object -First 3 | Format-Table -AutoSize
    Write-Host ""
} catch {
    Write-Host "note: could not fetch service categories" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "backedn api test done" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "backend  successfully connected to kubernetes sql server" -ForegroundColor Green
Write-Host ""
Write-Host "next: run failover test" -ForegroundColor Cyan
Write-Host "  cd ..\kubernetes-sqlserver" -ForegroundColor White
Write-Host "  .\comprehensive-failover-test.ps1" -ForegroundColor White
Write-Host ""
