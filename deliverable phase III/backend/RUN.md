# ServiceConnect API – Quick Run Notes

This app targets .NET 8. If only .NET 9 is installed, set roll-forward so it runs on 9.x.

## One-time setup
1) Copy env file:
   - `copy .env.example .env` (fill DB settings), and also copy into API folder so `Env.Load()` finds it:
   - `copy .env ServiceConnect.API\.env`
2) Trust HTTPS dev cert (for https://localhost:5001):
   - `dotnet dev-certs https --trust`

## Run (Development + roll-forward to 9)
From `deliverable phase III/backend`:
```
$env:DOTNET_ROLL_FORWARD="Major"
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"
dotnet run --project ServiceConnect.API
```

Swagger: http://localhost:5000/swagger (use HTTP to avoid browser cert prompts; HTTPS at 5001 works if the cert is trusted).

## Notes
- If you install the .NET 8 runtime/SDK, you can drop `DOTNET_ROLL_FORWARD`.
- Ensure SQL Server is up and reachable per `.env` (DB_SERVER/DB_NAME/auth).***/
