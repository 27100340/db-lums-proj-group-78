# PowerShell script to start backend and verify Kubernetes SQL Server connection

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "backend startup and connection test" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"

# check kubernetes sqlserver running
Write-Host "checking kubernetes sql erver pods" -ForegroundColor Yellow
$pods = kubectl get pods -n sqlserver-ha -l app=sqlserver -o jsonpath='{.items[*].metadata.name}'
if ([string]::IsNullOrEmpty($pods)) {
    Write-Host "no sql server pod found in kubernetes" -ForegroundColor Red
    Write-Host "use .\deploy.ps1 to deploy sql sever first" -ForegroundColor Yellow
    exit 1
}

kubectl get pods -n sqlserver-ha -l app=sqlserver
Write-Host ""

# test kubenetes sql connection
Write-Host "checking sql server connection" -ForegroundColor Yellow
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT 'SQL Server is ready' AS Status, DB_NAME() AS CurrentDatabase" 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "cannot connect to sql server" -ForegroundColor Red
    exit 1
}
Write-Host "sql server is good" -ForegroundColor Green
Write-Host ""

# cehck serviceconnect database exists
Write-Host "checking serviceconnect db" -ForegroundColor Yellow
$dbCheck = kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT name FROM sys.databases WHERE name = 'ServiceConnect'" -h -1 2>$null
if ([string]::IsNullOrWhiteSpace($dbCheck)) {
    Write-Host "serviceconnect db not found" -ForegroundColor Red
    Write-Host "use .\init-database.ps1 to initialize the database" -ForegroundColor Yellow
    exit 1
}
Write-Host "serviceconnect database exists" -ForegroundColor Green
Write-Host ""

# instructions for starting backend
Write-Host "starting backend" -ForegroundColor Yellow
Write-Host ""
Write-Host "note: run these commands in separate new  powershell window" -ForegroundColor Cyan
Write-Host ""
Write-Host "cd `"C:\Users\Baqir\Documents\LUMS\Academics\DB\Project\deliverable phase III\backend`"" -ForegroundColor White
Write-Host "`$env:DB_SERVER=`"localhost,31433`"" -ForegroundColor White
Write-Host "`$env:DOTNET_ROLL_FORWARD=`"Major`"" -ForegroundColor White
Write-Host "`$env:ASPNETCORE_ENVIRONMENT=`"Development`"" -ForegroundColor White
Write-Host "`$env:ASPNETCORE_URLS=`"https://localhost:5001;http://localhost:5000`"" -ForegroundColor White
Write-Host "dotnet run --project ServiceConnect.API" -ForegroundColor White
Write-Host ""

Write-Host "this startup message should show:" -ForegroundColor Yellow
Write-Host "  Database: ServiceConnect on localhost,31433" -ForegroundColor Cyan
Write-Host ""

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "after backend starts" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "test api with this command in another terminal" -ForegroundColor Yellow
Write-Host ""
Write-Host "Invoke-RestMethod -Uri `"http://localhost:5000/api/stats/counts`"" -ForegroundColor White
Write-Host ""
Write-Host "Or test service categories:" -ForegroundColor Yellow
Write-Host "Invoke-RestMethod -Uri `"http://localhost:5000/api/servicecategories`" | Select-Object -First 3" -ForegroundColor White
Write-Host ""

Write-Host "after backend working run failover test:" -ForegroundColor Cyan
Write-Host "  .\comprehensive-failover-test.ps1" -ForegroundColor White
Write-Host ""
