# PowerShell script to initialize the database in Kubernetes SQL Server

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "db intiialization" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

$sqlScriptPath = "..\ServiceConnect_Phase2_CORRECTED.sql"

# Check if SQL script exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "fault: sql script not found at $sqlScriptPath" -ForegroundColor Red
    Write-Host "ensure ServiceConnect_Phase2_CORRECTED.sql exists in the project root" -ForegroundColor Yellow
    exit 1
}

Write-Host "sql script found: $sqlScriptPath" -ForegroundColor Green
Write-Host ""

Write-Host "copy sql script to sqlserver-0 pod" -ForegroundColor Yellow
kubectl cp $sqlScriptPath sqlserver-ha/sqlserver-0:/tmp/init.sql
Write-Host "sql script copied" -ForegroundColor Green
Write-Host ""

Write-Host "executing SQL script on sqlserver-0..." -ForegroundColor Yellow
Write-Host "this step ususally takes time" -ForegroundColor Cyan
kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -i /tmp/init.sql

Write-Host ""
Write-Host "db initialized on sqlserver-0" -ForegroundColor Green
Write-Host ""

Write-Host "checking db" -ForegroundColor Yellow
$verifyQuery = "USE ServiceConnect; SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q $verifyQuery

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "db init done" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "db ready on" -ForegroundColor Cyan
Write-Host "  sqlserver-0 as the primary" -ForegroundColor White
Write-Host "  sqlserver-1 as secondary for failover)" -ForegroundColor White
Write-Host ""

Write-Host "note: sqlserver-1 is a separate instance it is not replicated" -ForegroundColor Yellow
Write-Host "for actual data replication you need to setup" -ForegroundColor Yellow
Write-Host "  sql server always on availability groups" -ForegroundColor White
Write-Host "  or apply manual database backup/restore to sqlserver-1" -ForegroundColor White
Write-Host ""

Write-Host "next" -ForegroundColor Cyan
Write-Host "  modify backend .env: DB_SERVER=localhost,31433" -ForegroundColor White
Write-Host ""
