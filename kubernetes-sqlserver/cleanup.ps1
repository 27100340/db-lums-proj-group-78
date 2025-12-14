# PowerShell script to cleanup SQL Server Kubernetes resources

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SQL Server Kubernetes Cleanup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "WARNING: This will delete all SQL Server resources from Kubernetes!" -ForegroundColor Red
Write-Host "This includes:" -ForegroundColor Yellow
Write-Host "  - All SQL Server pods" -ForegroundColor White
Write-Host "  - All persistent volumes (DATABASE DATA WILL BE LOST!)" -ForegroundColor White
Write-Host "  - All services" -ForegroundColor White
Write-Host "  - All secrets" -ForegroundColor White
Write-Host "  - The entire 'sqlserver-ha' namespace" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Are you sure you want to continue? Type 'yes' to confirm"

if ($confirm -ne "yes") {
    Write-Host "Cleanup cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Deleting sqlserver-ha namespace..." -ForegroundColor Yellow
kubectl delete namespace sqlserver-ha

Write-Host ""
Write-Host "Waiting for namespace deletion to complete..." -ForegroundColor Yellow
kubectl wait --for=delete namespace/sqlserver-ha --timeout=120s

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "All SQL Server resources have been removed." -ForegroundColor Green
Write-Host ""

Write-Host "To redeploy:" -ForegroundColor Cyan
Write-Host "  .\deploy.ps1" -ForegroundColor White
Write-Host ""
