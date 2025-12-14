# PowerShell script to initialize the database in Kubernetes SQL Server

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Initialization" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

$sqlScriptPath = "..\ServiceConnect_Phase2_CORRECTED.sql"

# Check if SQL script exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "Error: SQL script not found at $sqlScriptPath" -ForegroundColor Red
    Write-Host "Please ensure ServiceConnect_Phase2_CORRECTED.sql exists in the project root." -ForegroundColor Yellow
    exit 1
}

Write-Host "Found SQL script: $sqlScriptPath" -ForegroundColor Green
Write-Host ""

Write-Host "[1/3] Copying SQL script to sqlserver-0 pod..." -ForegroundColor Yellow
kubectl cp $sqlScriptPath sqlserver-ha/sqlserver-0:/tmp/init.sql
Write-Host "SQL script copied!" -ForegroundColor Green
Write-Host ""

Write-Host "[2/3] Executing SQL script on sqlserver-0..." -ForegroundColor Yellow
Write-Host "This may take several minutes (1.3M+ rows to insert)..." -ForegroundColor Cyan
kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -i /tmp/init.sql

Write-Host ""
Write-Host "Database initialized on sqlserver-0!" -ForegroundColor Green
Write-Host ""

Write-Host "[3/3] Verifying database..." -ForegroundColor Yellow
$verifyQuery = "USE ServiceConnect; SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q $verifyQuery

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Initialization Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Database is ready on:" -ForegroundColor Cyan
Write-Host "  - sqlserver-0 (Primary)" -ForegroundColor White
Write-Host "  - sqlserver-1 (Secondary - for failover)" -ForegroundColor White
Write-Host ""

Write-Host "Note: sqlserver-1 is a separate instance (not replicated)." -ForegroundColor Yellow
Write-Host "For true data replication, you would need to configure:" -ForegroundColor Yellow
Write-Host "  - SQL Server Always On Availability Groups" -ForegroundColor White
Write-Host "  - Or manual database backup/restore to sqlserver-1" -ForegroundColor White
Write-Host ""

Write-Host "Next step:" -ForegroundColor Cyan
Write-Host "  Update backend .env: DB_SERVER=localhost,31433" -ForegroundColor White
Write-Host ""
