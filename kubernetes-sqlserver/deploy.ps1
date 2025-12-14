# PowerShell script to deploy SQL Server to Kubernetes with High Availability

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "sql server high availabilty dpeloyment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Check if kubectl is available
try {
    kubectl version --client | Out-Null
} catch {
    Write-Host "fault: kubectl not found" -ForegroundColor Red
    exit 1
}

Write-Host "making namespace" -ForegroundColor Yellow
kubectl apply -f namespace.yaml
Write-Host ""

Write-Host "making secret" -ForegroundColor Yellow
kubectl apply -f secret.yaml
Write-Host ""

Write-Host "making SQL Server replicas(2)" -ForegroundColor Yellow
kubectl apply -f sqlserver-statefulset.yaml
Write-Host ""

Write-Host "making sql server services" -ForegroundColor Yellow
kubectl apply -f sqlserver-service.yaml
Write-Host ""

Write-Host "making poddisruptionbudget" -ForegroundColor Yellow
kubectl apply -f poddisruptionbudget.yaml
Write-Host "make poddistributionbudget!" -ForegroundColor Green
Write-Host ""

Write-Host "wait for SQL pods to initialize" -ForegroundColor Yellow
Write-Host "this step will take time" -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=sqlserver -n sqlserver-ha --timeout=300s

Write-Host ""
Write-Host "=============================" -ForegroundColor Cyan
Write-Host "deploying done" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "sql  Status:" -ForegroundColor Cyan
kubectl get pods -n sqlserver-ha
Write-Host ""

Write-Host "connection info" -ForegroundColor Cyan
Write-Host "  Host: localhost" -ForegroundColor White
Write-Host "  Port: 31433" -ForegroundColor White
Write-Host "  User: sa" -ForegroundColor White
Write-Host "  Password: YourStrong!Passw0rd" -ForegroundColor White
Write-Host ""

Write-Host "change the backend .env file with" -ForegroundColor Yellow
Write-Host "  DB_SERVER=localhost,31433" -ForegroundColor White
Write-Host "  DB_USER=sa" -ForegroundColor White
Write-Host "  DB_PASSWORD=YourStrong!Passw0rd" -ForegroundColor White
Write-Host ""

Write-Host "Test connection:" -ForegroundColor Cyan
Write-Host "  kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q 'SELECT @@VERSION'" -ForegroundColor White
Write-Host ""


Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. start database using .\init-database.ps1" -ForegroundColor White
Write-Host "  2. update the backend .env with the connection info above" -ForegroundColor White
Write-Host "  3. to test failover .\test-failover.ps1" -ForegroundColor White
Write-Host ""
