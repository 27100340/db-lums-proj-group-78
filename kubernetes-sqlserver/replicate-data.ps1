# PowerShell script to replicate data from sqlserver-0 to sqlserver-1

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "replicating data: sqlserver-0 → sqlserver-1" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

Write-Host "making backup on sqlserver-0" -ForegroundColor Yellow
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "BACKUP DATABASE ServiceConnect TO DISK = '/var/opt/mssql/data/backup.bak' WITH FORMAT, INIT"
Write-Host "backup made" -ForegroundColor Green
Write-Host ""

Write-Host "copy backup from sqlserver-0 to local machine" -ForegroundColor Yellow
kubectl cp sqlserver-ha/sqlserver-0:/var/opt/mssql/data/backup.bak ./backup.bak
Write-Host "backup made" -ForegroundColor Green
Write-Host ""

Write-Host "copy the backup to sqlserver-1" -ForegroundColor Yellow
kubectl cp ./backup.bak sqlserver-ha/sqlserver-1:/var/opt/mssql/data/backup.bak
Write-Host "backup has eben uploaded to sqlserver-1" -ForegroundColor Green
Write-Host ""

Write-Host " restore the database on sqlserver-1" -ForegroundColor Yellow
Write-Host "this takes some time" -ForegroundColor Cyan
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "RESTORE DATABASE ServiceConnect FROM DISK = '/var/opt/mssql/data/backup.bak' WITH REPLACE"
Write-Host ""

Write-Host "check data on both pods" -ForegroundColor Yellow
$verifyQuery = "USE ServiceConnect; SELECT COUNT(*) AS TotalRows FROM (SELECT * FROM Users UNION ALL SELECT * FROM Workers UNION ALL SELECT * FROM Customers) AS AllData;"

Write-Host ""
Write-Host "sqlserver-0 data:" -ForegroundColor Cyan
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Workers) + (SELECT COUNT(*) FROM Customers) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) + (SELECT COUNT(*) FROM Bookings) AS TotalRows"

Write-Host ""
Write-Host "sqlserver-1 data:" -ForegroundColor Cyan
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Workers) + (SELECT COUNT(*) FROM Customers) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) + (SELECT COUNT(*) FROM Bookings) AS TotalRows"

Write-Host ""
Write-Host "celan the local backup" -ForegroundColor Yellow
Remove-Item ./backup.bak -Force
Write-Host "cleanup done" -ForegroundColor Green
Write-Host ""

Write-Host "data duplicating done" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "both sql pods should have identical data now" -ForegroundColor Green
Write-Host " sqlserver-0 should have the full database" -ForegroundColor White
Write-Host "  - sqlserver-1 full database should be replicated" -ForegroundColor White
Write-Host ""

Write-Host "we can now do the failover tesr" -ForegroundColor Cyan
Write-Host "  .\comprehensive-failover-test.ps1" -ForegroundColor White
Write-Host ""
