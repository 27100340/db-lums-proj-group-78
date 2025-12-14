# Test Backend API Connection

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Backend API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"

Write-Host "Testing API endpoint: http://localhost:5000/api/stats/counts" -ForegroundColor Yellow
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/stats/counts" -Method Get
    Write-Host "✓ SUCCESS! Backend is connected to Kubernetes SQL Server!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Database Statistics:" -ForegroundColor Cyan
    $response | Format-List
    Write-Host ""
} catch {
    Write-Host "✗ FAILED: Cannot connect to backend" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error Details:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host ""
    Write-Host "Make sure:" -ForegroundColor Yellow
    Write-Host "  1. Backend is running (run start-backend.ps1)" -ForegroundColor White
    Write-Host "  2. Backend shows 'Database: ServiceConnect on localhost,31433'" -ForegroundColor White
    Write-Host "  3. No firewall blocking port 5000" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "Testing another endpoint: http://localhost:5000/api/servicecategories" -ForegroundColor Yellow
Write-Host ""

try {
    $categories = Invoke-RestMethod -Uri "http://localhost:5000/api/servicecategories" -Method Get
    Write-Host "✓ Service Categories Retrieved!" -ForegroundColor Green
    Write-Host ""
    Write-Host "First 3 Categories:" -ForegroundColor Cyan
    $categories | Select-Object -First 3 | Format-Table -AutoSize
    Write-Host ""
} catch {
    Write-Host "✗ Warning: Could not retrieve service categories" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend API Test Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your backend is successfully connected to Kubernetes SQL Server!" -ForegroundColor Green
Write-Host ""
Write-Host "Next step: Run the comprehensive failover test" -ForegroundColor Cyan
Write-Host "  cd ..\kubernetes-sqlserver" -ForegroundColor White
Write-Host "  .\comprehensive-failover-test.ps1" -ForegroundColor White
Write-Host ""
