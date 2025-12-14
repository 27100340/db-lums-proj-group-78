# PowerShell script to deploy SQL Server to Kubernetes with High Availability

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SQL Server High Availability Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Check if kubectl is available
try {
    kubectl version --client | Out-Null
} catch {
    Write-Host "Error: kubectl not found. Please install kubectl first." -ForegroundColor Red
    exit 1
}

Write-Host "[1/5] Creating namespace..." -ForegroundColor Yellow
kubectl apply -f namespace.yaml
Write-Host "Namespace created!" -ForegroundColor Green
Write-Host ""

Write-Host "[2/5] Creating secret..." -ForegroundColor Yellow
kubectl apply -f secret.yaml
Write-Host "Secret created!" -ForegroundColor Green
Write-Host ""

Write-Host "[3/5] Deploying SQL Server StatefulSet (2 replicas)..." -ForegroundColor Yellow
kubectl apply -f sqlserver-statefulset.yaml
Write-Host "SQL Server StatefulSet created!" -ForegroundColor Green
Write-Host ""

Write-Host "[4/5] Creating SQL Server services..." -ForegroundColor Yellow
kubectl apply -f sqlserver-service.yaml
Write-Host "Services created!" -ForegroundColor Green
Write-Host ""

Write-Host "[5/5] Creating PodDisruptionBudget..." -ForegroundColor Yellow
kubectl apply -f poddisruptionbudget.yaml
Write-Host "PodDisruptionBudget created!" -ForegroundColor Green
Write-Host ""

Write-Host "Waiting for SQL Server pods to be ready..." -ForegroundColor Yellow
Write-Host "This may take 2-3 minutes..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=sqlserver -n sqlserver-ha --timeout=300s

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "SQL Server Status:" -ForegroundColor Cyan
kubectl get pods -n sqlserver-ha
Write-Host ""

Write-Host "Connection Information:" -ForegroundColor Cyan
Write-Host "  Host: localhost" -ForegroundColor White
Write-Host "  Port: 31433" -ForegroundColor White
Write-Host "  User: sa" -ForegroundColor White
Write-Host "  Password: YourStrong!Passw0rd" -ForegroundColor White
Write-Host ""

Write-Host "Update your backend .env file with:" -ForegroundColor Yellow
Write-Host "  DB_SERVER=localhost,31433" -ForegroundColor White
Write-Host "  DB_USER=sa" -ForegroundColor White
Write-Host "  DB_PASSWORD=YourStrong!Passw0rd" -ForegroundColor White
Write-Host ""

Write-Host "Test connection:" -ForegroundColor Cyan
Write-Host "  kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q 'SELECT @@VERSION'" -ForegroundColor White
Write-Host ""

Write-Host "High Availability Features:" -ForegroundColor Cyan
Write-Host "  - 2 SQL Server replicas running" -ForegroundColor Green
Write-Host "  - Each with 10Gi persistent storage" -ForegroundColor Green
Write-Host "  - Automatic pod recovery on failure" -ForegroundColor Green
Write-Host "  - PodDisruptionBudget ensures 1 pod always available" -ForegroundColor Green
Write-Host "  - Health checks for automatic restart" -ForegroundColor Green
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Initialize database: .\init-database.ps1" -ForegroundColor White
Write-Host "  2. Update backend .env with the connection info above" -ForegroundColor White
Write-Host "  3. Test failover: .\test-failover.ps1" -ForegroundColor White
Write-Host ""
