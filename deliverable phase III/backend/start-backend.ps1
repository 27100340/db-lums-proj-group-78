# Start Backend Connected to Kubernetes SQL Server

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting Backend with Kubernetes SQL Server" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
$env:DB_SERVER="localhost,31433"
$env:DOTNET_ROLL_FORWARD="Major"
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"

Write-Host "Environment Variables Set:" -ForegroundColor Yellow
Write-Host "  DB_SERVER: $env:DB_SERVER" -ForegroundColor White
Write-Host "  ASPNETCORE_URLS: $env:ASPNETCORE_URLS" -ForegroundColor White
Write-Host ""

Write-Host "Starting backend..." -ForegroundColor Yellow
Write-Host "Look for: 'Database: ServiceConnect on localhost,31433'" -ForegroundColor Cyan
Write-Host ""

# Run the backend
dotnet run --project ServiceConnect.API
