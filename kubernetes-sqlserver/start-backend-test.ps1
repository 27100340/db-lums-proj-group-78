# PowerShell script to start backend and verify Kubernetes SQL Server connection

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend Startup and Connection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"

# Step 1: Verify Kubernetes SQL Server is running
Write-Host "[1/4] Verifying Kubernetes SQL Server pods..." -ForegroundColor Yellow
$pods = kubectl get pods -n sqlserver-ha -l app=sqlserver -o jsonpath='{.items[*].metadata.name}'
if ([string]::IsNullOrEmpty($pods)) {
    Write-Host "ERROR: No SQL Server pods found in Kubernetes!" -ForegroundColor Red
    Write-Host "Run: .\deploy.ps1 to deploy SQL Server first" -ForegroundColor Yellow
    exit 1
}

kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

# Step 2: Test Kubernetes SQL Server connectivity
Write-Host "[2/4] Testing SQL Server connectivity..." -ForegroundColor Yellow
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT 'SQL Server is ready' AS Status, DB_NAME() AS CurrentDatabase" 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Cannot connect to SQL Server!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ SQL Server is responding" -ForegroundColor Green
Write-Host ""

# Step 3: Verify ServiceConnect database exists
Write-Host "[3/4] Verifying ServiceConnect database..." -ForegroundColor Yellow
$dbCheck = kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT name FROM sys.databases WHERE name = 'ServiceConnect'" -h -1 2>$null
if ([string]::IsNullOrWhiteSpace($dbCheck)) {
    Write-Host "ERROR: ServiceConnect database not found!" -ForegroundColor Red
    Write-Host "Run: .\init-database.ps1 to initialize the database" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ ServiceConnect database exists" -ForegroundColor Green
Write-Host ""

# Step 4: Instructions for starting backend
Write-Host "[4/4] Starting Backend..." -ForegroundColor Yellow
Write-Host ""
Write-Host "IMPORTANT: Run these commands in a NEW PowerShell window:" -ForegroundColor Cyan
Write-Host ""
Write-Host "cd `"C:\Users\Baqir\Documents\LUMS\Academics\DB\Project\deliverable phase III\backend`"" -ForegroundColor White
Write-Host "`$env:DB_SERVER=`"localhost,31433`"" -ForegroundColor White
Write-Host "`$env:DOTNET_ROLL_FORWARD=`"Major`"" -ForegroundColor White
Write-Host "`$env:ASPNETCORE_ENVIRONMENT=`"Development`"" -ForegroundColor White
Write-Host "`$env:ASPNETCORE_URLS=`"https://localhost:5001;http://localhost:5000`"" -ForegroundColor White
Write-Host "dotnet run --project ServiceConnect.API" -ForegroundColor White
Write-Host ""

Write-Host "Expected startup message should show:" -ForegroundColor Yellow
Write-Host "  Database: ServiceConnect on localhost,31433" -ForegroundColor Cyan
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "After Backend Starts" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test the API with this command (in another terminal):" -ForegroundColor Yellow
Write-Host ""
Write-Host "Invoke-RestMethod -Uri `"http://localhost:5000/api/stats/counts`"" -ForegroundColor White
Write-Host ""
Write-Host "Or test service categories:" -ForegroundColor Yellow
Write-Host "Invoke-RestMethod -Uri `"http://localhost:5000/api/servicecategories`" | Select-Object -First 3" -ForegroundColor White
Write-Host ""

Write-Host "Once backend is working, run the failover test:" -ForegroundColor Cyan
Write-Host "  .\comprehensive-failover-test.ps1" -ForegroundColor White
Write-Host ""
