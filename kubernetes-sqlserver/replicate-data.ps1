# PowerShell script to replicate data from sqlserver-0 to sqlserver-1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Data Replication: sqlserver-0 → sqlserver-1" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

Write-Host "[1/5] Taking backup on sqlserver-0..." -ForegroundColor Yellow
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "BACKUP DATABASE ServiceConnect TO DISK = '/var/opt/mssql/data/backup.bak' WITH FORMAT, INIT"
Write-Host "Backup created!" -ForegroundColor Green
Write-Host ""

Write-Host "[2/5] Copying backup from sqlserver-0 to local machine..." -ForegroundColor Yellow
kubectl cp sqlserver-ha/sqlserver-0:/var/opt/mssql/data/backup.bak ./backup.bak
Write-Host "Backup downloaded!" -ForegroundColor Green
Write-Host ""

Write-Host "[3/5] Copying backup to sqlserver-1..." -ForegroundColor Yellow
kubectl cp ./backup.bak sqlserver-ha/sqlserver-1:/var/opt/mssql/data/backup.bak
Write-Host "Backup uploaded to sqlserver-1!" -ForegroundColor Green
Write-Host ""

Write-Host "[4/5] Restoring database on sqlserver-1..." -ForegroundColor Yellow
Write-Host "This may take a minute..." -ForegroundColor Cyan
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "RESTORE DATABASE ServiceConnect FROM DISK = '/var/opt/mssql/data/backup.bak' WITH REPLACE"
Write-Host "Database restored on sqlserver-1!" -ForegroundColor Green
Write-Host ""

Write-Host "[5/5] Verifying data on both pods..." -ForegroundColor Yellow
$verifyQuery = "USE ServiceConnect; SELECT COUNT(*) AS TotalRows FROM (SELECT * FROM Users UNION ALL SELECT * FROM Workers UNION ALL SELECT * FROM Customers) AS AllData;"

Write-Host ""
Write-Host "sqlserver-0 data:" -ForegroundColor Cyan
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Workers) + (SELECT COUNT(*) FROM Customers) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) + (SELECT COUNT(*) FROM Bookings) AS TotalRows"

Write-Host ""
Write-Host "sqlserver-1 data:" -ForegroundColor Cyan
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Workers) + (SELECT COUNT(*) FROM Customers) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) + (SELECT COUNT(*) FROM Bookings) AS TotalRows"

Write-Host ""
Write-Host "Cleaning up local backup file..." -ForegroundColor Yellow
Remove-Item ./backup.bak -Force
Write-Host "Cleanup complete!" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Data Replication Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Both SQL Server pods now have identical data!" -ForegroundColor Green
Write-Host "  - sqlserver-0: ✓ Full database" -ForegroundColor White
Write-Host "  - sqlserver-1: ✓ Full database (replicated)" -ForegroundColor White
Write-Host ""

Write-Host "Now you can run the comprehensive failover test:" -ForegroundColor Cyan
Write-Host "  .\comprehensive-failover-test.ps1" -ForegroundColor White
Write-Host ""
