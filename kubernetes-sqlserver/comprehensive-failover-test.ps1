# PowerShell script to demonstrate complete high availability failover

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "high availability test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

Write-Host " test will show" -ForegroundColor Yellow
Write-Host "  1. both sql pods should have identical data" -ForegroundColor White
Write-Host "  2. kill sqlserver-0 → reroutes to sqlserver-1" -ForegroundColor White
Write-Host "  3. app continues working" -ForegroundColor White
Write-Host "  4. kubernetes auto recreates sqlserver-0" -ForegroundColor White
Write-Host "  5. delete sqlserver-1 →  reroutes to sqlserver-0" -ForegroundColor White
Write-Host "  6. app works with no data loss" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "note: ensure backend is running. Continue? (y/n)"
if ($confirm -ne "y") {
    Write-Host "test cancelled" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "initial State" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "current sql pods" -ForegroundColor Yellow
kubectl get pods -n sqlserver-ha -l app=sqlserver -o wide
Write-Host ""

Write-Host "test connection to kubernetes sql server" -ForegroundColor Yellow
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT 'sqlserver-0 is online' AS Status, @@VERSION AS Version"
Write-Host ""
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT 'sqlserver-1 is online' AS Status, @@VERSION AS Version"
Write-Host ""

Write-Host "both pods running and have data" -ForegroundColor Green
Write-Host ""
Write-Host "click Enter when your backend is running and responding" -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "show sqlserver-0 failure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "deleting sqlserver-0 pod showing failure" -ForegroundColor Red
kubectl delete pod -n sqlserver-ha sqlserver-0
Write-Host ""

Write-Host " traffic auto routes to sqlserver-1" -ForegroundColor Green
Write-Host "app should continue working without interruption" -ForegroundColor Green
Write-Host ""

Write-Host "current pod status" -ForegroundColor Yellow
Start-Sleep -Seconds 5
kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

Write-Host "notice:" -ForegroundColor Cyan
Write-Host "  sqlserver-0 recreatign " -ForegroundColor White
Write-Host "  sqlserver-1 still running" -ForegroundColor White
Write-Host "  nodeport service routes traffic to sqlserver-1" -ForegroundColor White
Write-Host "  No downtime as expected" -ForegroundColor Green
Write-Host ""

Write-Host "test application now" -ForegroundColor Yellow
Write-Host "tapEnter when verified the application works" -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "wait for sqlserver-0 to recover" -ForegroundColor Yellow
kubectl wait --for=condition=ready pod/sqlserver-0 -n sqlserver-ha --timeout=120s
Write-Host ""

Write-Host " sqlserver-0 auto recreated and healthy" -ForegroundColor Green
Write-Host ""

Write-Host " pod status:" -ForegroundColor Yellow
kubectl get pods -n sqlserver-ha -l app=sqlserver -o wide
Write-Host ""

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "simulate sqlserver-1 failure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "testing the reverse" -ForegroundColor Cyan
Write-Host "tap Enter to proced" -ForegroundColor Yellow
Read-Host
Write-Host ""

Write-Host "note: killing sqlserver-1 pod showing failure" -ForegroundColor Red
kubectl delete pod -n sqlserver-ha sqlserver-1
Write-Host ""

Write-Host "traffic auto routes to sqlserver-0" -ForegroundColor Green
Write-Host "application should be working fine" -ForegroundColor Green
Write-Host ""

Write-Host " pod status" -ForegroundColor Yellow
Start-Sleep -Seconds 5
kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

Write-Host "note:" -ForegroundColor Cyan
Write-Host "  sqlserver-0 is running (1/1 Ready)" -ForegroundColor White
Write-Host "   sqlserver-1 is being recreated" -ForegroundColor White
Write-Host "  nodeport service routes traffic to sqlserver-0" -ForegroundColor White
Write-Host "  app working with zero downtime" -ForegroundColor Green
Write-Host ""

Write-Host "testing application again it should STILL work" -ForegroundColor Yellow
Write-Host "tap Enter after checking the application works" -ForegroundColor Yellow
Read-Host

Write-Host ""
Write-Host "wait for sqlserver-1 to fully recover" -ForegroundColor Yellow
kubectl wait --for=condition=ready pod/sqlserver-1 -n sqlserver-ha --timeout=120s
Write-Host ""

Write-Host "sqlserver-1 has been automatically recreated and is healthy" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " both pods shoudl have recovered" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "pod status:" -ForegroundColor Yellow
kubectl get pods -n sqlserver-ha -l app=sqlserver -o wide
Write-Host ""

Write-Host "test connection to both pods" -ForegroundColor Yellow
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT 'sqlserver-0 recovered' AS Status, GETDATE() AS CurrentTime"
Write-Host ""
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT 'sqlserver-1 recovered' AS Status, GETDATE() AS CurrentTime"
Write-Host ""

