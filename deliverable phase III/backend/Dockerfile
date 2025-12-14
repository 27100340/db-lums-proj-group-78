# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ServiceConnect.sln .
COPY ServiceConnect.API/ServiceConnect.API.csproj ServiceConnect.API/
COPY ServiceConnect.BLL/ServiceConnect.BLL.csproj ServiceConnect.BLL/
COPY ServiceConnect.Models/ServiceConnect.Models.csproj ServiceConnect.Models/

# Restore dependencies
RUN dotnet restore ServiceConnect.sln

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/ServiceConnect.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Copy .env file (optional, can use ConfigMap instead)
COPY .env.example .env

# Expose ports
EXPOSE 5000 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ROLL_FORWARD=Major

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl --fail http://localhost:5000/api/servicecategories || exit 1

# Run the application
ENTRYPOINT ["dotnet", "ServiceConnect.API.dll"]
