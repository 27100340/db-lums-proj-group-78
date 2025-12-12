# ServiceConnect API running method

app targets .NET 8
only .NET 9 installed set roll-forward
then it runs on 9.x

## first setup

1. Copy env file:

     copy .env.example .env
     intialize DB settings and also copy into API folder so `Env.Load() findds it:
     copy .env ServiceConnect.API.env
2. trust HTTPS dev cert:
   dotnet dev-certs https --trust

## do (from /backend folder root)

dotnet restore
dotnet build

## Run (Development + roll-forward to 9)

from deliverable phase III/backend

```powershell
$env:DOTNET_ROLL_FORWARD="Major"
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"
dotnet run --project ServiceConnect.API
```

Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger) (use HTTP to avoid browser cert prompts; HTTPS at 5001 works if the cert is trusted)

## Notes

if install the .NET 8 runtime/SDK you can drop `DOTNET_ROLL_FORWARD`.
make sure SQL Server is up and reachable per `.env` (DB_SERVER/DB_NAME/auth).
