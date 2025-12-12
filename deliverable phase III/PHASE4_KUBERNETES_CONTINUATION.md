# Phase 4: Kubernetes Deployment – Continuation Guide

**For Extra Credit Implementation**
**Project**: ServiceConnect Database Application

---
## Phase 4 Overview: Kubernetes Deployment

### Objectives

1. Containerize all application components using Docker
2. Deploy the system to a Kubernetes cluster
3. Implement horizontal pod autoscaling
4. Configure persistent storage for SQL Server
5. Enable service discovery and load balancing
6. Set up monitoring and logging

### Architecture

```
Kubernetes Cluster
├── Namespace: serviceconnect
│
├── Deployments:
│   ├── backend-deployment (ASP.NET Core API)
│   │   ├── Replicas: 3 (auto-scaling 2–5)
│   │   ├── Image: serviceconnect-api:latest
│   │   └── Port: 5000
│   │
│   ├── frontend-deployment (Next.js)
│   │   ├── Replicas: 2 (auto-scaling 1–3)
│   │   ├── Image: serviceconnect-ui:latest
│   │   └── Port: 3000
│   │   │
│   └── database-deployment (SQL Server)
│       ├── Replicas: 1 (StatefulSet)
│       ├── Image: mcr.microsoft.com/mssql/server:2022-latest
│       └── Port: 1433
│
├── Services:
│   ├── backend-service (ClusterIP / LoadBalancer)
│   ├── frontend-service (LoadBalancer)
│   └── database-service (ClusterIP)
│
├── ConfigMaps:
│   ├── app-config (API configuration)
│   └── database-init (SQL initialization scripts)
│
├── Secrets:
│   ├── database-credentials
│   └── api-secrets
│
├── Persistent Volumes:
│   └── sql-data-pv (SQL Server data persistence)
│
└── Ingress:
    └── serviceconnect-ingress (External access routing)
```

---

## Step-by-Step Implementation Guide

### Step 1: Containerize Applications with Docker

#### 1.1 Dockerfile for Backend (ASP.NET Core)

**File**: `backend/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ServiceConnect.sln ./
COPY ServiceConnect.API/ServiceConnect.API.csproj ServiceConnect.API/
COPY ServiceConnect.BLL/ServiceConnect.BLL.csproj ServiceConnect.BLL/
COPY ServiceConnect.Models/ServiceConnect.Models.csproj ServiceConnect.Models/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build application
WORKDIR /src/ServiceConnect.API
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 5000
EXPOSE 5001

# Environment configuration
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "ServiceConnect.API.dll"]
```

#### 1.2 Dockerfile for Frontend (Next.js)

**File**: `frontend/serviceconnect-ui/Dockerfile`

```dockerfile
# Dependencies stage
FROM node:18-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

# Build stage
FROM node:18-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build Next.js app
ENV NEXT_TELEMETRY_DISABLED 1
RUN npm run build

# Production stage
FROM node:18-alpine AS runner
WORKDIR /app

ENV NODE_ENV production
ENV NEXT_TELEMETRY_DISABLED 1

# Create non-root user
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy built output
COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 3000

ENV PORT 3000
ENV HOSTNAME "0.0.0.0"

CMD ["node", "server.js"]
```

#### 1.3 Docker Compose for Local Testing

**File**: `docker-compose.yml`

```yaml
version: '3.8'

services:
  database:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - serviceconnect-network

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    environment:
      - DB_SERVER=database
      - DB_NAME=ServiceConnect
      - DB_USER=sa
      - DB_PASSWORD=YourStrong!Passw0rd
      - DB_INTEGRATED_SECURITY=false
      - BLL_TYPE=LinqEF
    ports:
      - "5000:5000"
    depends_on:
      - database
    networks:
      - serviceconnect-network

  frontend:
    build:
      context: ./frontend/serviceconnect-ui
      dockerfile: Dockerfile
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:5000/api
    ports:
      - "3000:3000"
    depends_on:
      - backend
    networks:
      - serviceconnect-network

volumes:
  sqldata:

networks:
  serviceconnect-network:
    driver: bridge
```

---

### Step 2: Kubernetes Configuration Files

#### 2.1 Namespace

**File**: `k8s/namespace.yaml`

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: serviceconnect
```

#### 2.2 SQL Server StatefulSet

**File**: `k8s/database-statefulset.yaml`

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: mssql-secret
  namespace: serviceconnect
type: Opaque
data:
  SA_PASSWORD: WW91clN0cm9uZyFQYXNzdzByZA==

---

apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: mssql-pvc
  namespace: serviceconnect
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 50Gi

---

apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: mssql
  namespace: serviceconnect
spec:
  serviceName: mssql-service
  replicas: 1
  selector:
    matchLabels:
      app: mssql
  template:
    metadata:
      labels:
        app: mssql
    spec:
      containers:
      - name: mssql
        image: mcr.microsoft.com/mssql/server:2022-latest
        ports:
        - containerPort: 1433
        env:
        - name: ACCEPT_EULA
          value: "Y"
        - name: SA_PASSWORD
          valueFrom:
            secretKeyRef:
              name: mssql-secret
              key: SA_PASSWORD
        - name: MSSQL_PID
          value: "Developer"
        volumeMounts:
        - name: mssql-storage
          mountPath: /var/opt/mssql
        resources:
          requests:
            memory: "2Gi"
            cpu: "1000m"
          limits:
            memory: "4Gi"
            cpu: "2000m"
      volumes:
      - name: mssql-storage
        persistentVolumeClaim:
          claimName: mssql-pvc

---

apiVersion: v1
kind: Service
metadata:
  name: mssql-service
  namespace: serviceconnect
spec:
  selector:
    app: mssql
  ports:
  - protocol: TCP
    port: 1433
    targetPort: 1433
  type: ClusterIP
```

---