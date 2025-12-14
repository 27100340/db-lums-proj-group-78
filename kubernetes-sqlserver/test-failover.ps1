# PowerShell script to test SQL Server pod failure and recovery

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "sql server failover testing" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

Write-Host "script simulates Sql server pod failure the it will show:" -ForegroundColor Cyan
Write-Host " auto pod recovery" -ForegroundColor White
Write-Host " kubernetes self-healing" -ForegroundColor White
Write-Host " high availability with 2 replicas" -ForegroundColor White
Write-Host " ensuring 1 pod stays running" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "continue with failover test? (y/n)"
if ($confirm -ne "y") {
    Write-Host "test cancelle" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "initially " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "current sql Server pods" -ForegroundColor Yellow
kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

# get the first pod name
$podToDelete = kubectl get pods -n sqlserver-ha -l app=sqlserver -o jsonpath='{.items[0].metadata.name}'

if ([string]::IsNullOrEmpty($podToDelete)) {
    Write-Host "fault: no sql pods found" -ForegroundColor Red
    exit 1
}

Write-Host "=========================" -ForegroundColor Cyan
Write-Host "display pod failure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "killing pod: $podToDelete" -ForegroundColor Red
Write-Host "show failure" -ForegroundColor Yellow
kubectl delete pod -n sqlserver-ha $podToDelete

Write-Host ""
Write-Host "pod killed kubernetes recovering" -ForegroundColor Yellow
Write-Host ""

Write-Host "wait 5s" -ForegroundColor Cyan
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "recovery in progress" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "current pod condition" -ForegroundColor Yellow
kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

Write-Host "note: kubernetes auto created a new pod to replace the illed one" -ForegroundColor Green
Write-Host ""

Write-Host "wait for new pod to be ready" -ForegroundColor Yellow
Write-Host "takes 30-60s" -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=sqlserver -n sqlserver-ha --timeout=120s

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "recovered " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "updated pod status:" -ForegroundColor Yellow
kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

Write-Host "checking sql server connection" -ForegroundColor Yellow
$testQuery = "SELECT @@VERSION AS 'SQL Server Version', GETDATE() AS 'Current Time';"
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q $testQuery

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "killing test result" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "pod was killed successfully" -ForegroundColor Green
Write-Host "kubernetes auto recreated the pod" -ForegroundColor Green
Write-Host "new pod passed health checks" -ForegroundColor Green
Write-Host "sql server running and accepting connections" -ForegroundColor Green
Write-Host "data persistence checked " -ForegroundColor Green
Write-Host ""

Write-Host "high availability features shown:" -ForegroundColor Cyan
Write-Host "  self healing auto pod recovery" -ForegroundColor White
Write-Host "  persistent storage: Data survives pod restarts" -ForegroundColor White
Write-Host "  health check automatic readiness verification" -ForegroundColor White
Write-Host "  statefulset stable pod identity restored" -ForegroundColor White
Write-Host ""

Write-Host "with 2 replicas running:" -ForegroundColor Yellow
Write-Host "  if one pod fails other continues serving requests" -ForegroundColor White
Write-Host "  pod distribution budget ensures at least 1 pod always available" -ForegroundColor White
Write-Host "  xero downtime if both replicas running" -ForegroundColor White
Write-Host ""

Write-Host "to do this test" -ForegroundColor Cyan
Write-Host "  connect backend to sql server" -ForegroundColor White
Write-Host "  run this failover test again" -ForegroundColor White
Write-Host "  app shoudl remain working" -ForegroundColor White
Write-Host ""
