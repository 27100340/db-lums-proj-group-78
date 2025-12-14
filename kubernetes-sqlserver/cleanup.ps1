# PowerShell script to cleanup SQL Server Kubernetes resources

Write-Host "sql server kubernetes cleaning" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "note: this deletes all sql resources from kubernetes!" -ForegroundColor Red
Write-Host "including" -ForegroundColor Yellow
Write-Host "  all pods" -ForegroundColor White
Write-Host "  all persistent volumes (aka db data lost)" -ForegroundColor White
Write-Host "  all services" -ForegroundColor White
Write-Host "  all secrets" -ForegroundColor White
Write-Host "  entire 'sqlserver-ha' namespace" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "are you sure you want to continue? Type 'yes' to confirm"

if ($confirm -ne "yes") {
    Write-Host "cleanup cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "deleting sqlserver-ha namespace" -ForegroundColor Yellow
kubectl delete namespace sqlserver-ha

Write-Host ""
Write-Host "waiting for namespace deletion to complete" -ForegroundColor Yellow
kubectl wait --for=delete namespace/sqlserver-ha --timeout=120s

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "cleanup done" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "sql server resources have been removed." -ForegroundColor Green
Write-Host ""

Write-Host "to deploy agian" -ForegroundColor Cyan
Write-Host "  .\deploy.ps1" -ForegroundColor White
Write-Host ""
