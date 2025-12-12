# Phase 4: Kubernetes Deployment - Continuation Guide
**For Extra Credit Implementation**
**Project**: ServiceConnect Database Application
**Current Phase**: 3 (Complete)
**Next Phase**: 4 (Kubernetes - Optional Extra Credit)

---

## Prerequisites Before Starting Phase 4
This guide assumes you have completed Phase 3 with:
- ✅ Working ASP.NET Core Web API backend
- ✅ Working Next.js frontend
- ✅ SQL Server database with 1.3M+ rows
- ✅ Dual BLL implementations (LINQ/EF and Stored Procedures)
- ✅ Factory Pattern implementation

---

## Phase 4 Overview: Kubernetes Deployment

### Objectives
1. Containerize all application components using Docker
2. Deploy to Kubernetes cluster
3. Implement horizontal pod autoscaling
4. Configure persistent storage for SQL Server
5. Implement service discovery and load balancing
6. Set up monitoring and logging

### Architecture

```
Kubernetes Cluster
├── Namespace: serviceconnect
│
├── Deployments:
│   ├── backend-deployment (ASP.NET Core API)
│   │   ├── Replicas: 3 (auto-scaling 2-5)
│   │   ├── Image: serviceconnect-api:latest
│   │   └── Port: 5000
│   │
│   ├── frontend-deployment (Next.js)
│   │   ├── Replicas: 2 (auto-scaling 1-3)
│   │   ├── Image: serviceconnect-ui:latest
│   │   └── Port: 3000
│   │
│   └── database-deployment (SQL Server)
│       ├── Replicas: 1 (StatefulSet)
│       ├── Image: mcr.microsoft.com/mssql/server:2022-latest
│       └── Port: 1433
│
├── Services:
│   ├── backend-service (ClusterIP/LoadBalancer)
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

#### 1.1 Create Dockerfile for Backend (ASP.NET Core)

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

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "ServiceConnect.API.dll"]
```

#### 1.2 Create Dockerfile for Frontend (Next.js)

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

# Add non-root user
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy built application
COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 3000

ENV PORT 3000
ENV HOSTNAME "0.0.0.0"

CMD ["node", "server.js"]
```

#### 1.3 Create docker-compose.yml for Local Testing

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
  SA_PASSWORD: WW91clN0cm9uZyFQYXNzdzByZA== # Base64: YourStrong!Passw0rd

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

#### 2.3 Backend Deployment

**File**: `k8s/backend-deployment.yaml`

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: backend-config
  namespace: serviceconnect
data:
  DB_SERVER: "mssql-service"
  DB_NAME: "ServiceConnect"
  BLL_TYPE: "LinqEF"

---

apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend
  namespace: serviceconnect
spec:
  replicas: 3
  selector:
    matchLabels:
      app: backend
  template:
    metadata:
      labels:
        app: backend
    spec:
      containers:
      - name: backend
        image: <your-registry>/serviceconnect-api:latest
        ports:
        - containerPort: 5000
        env:
        - name: DB_SERVER
          valueFrom:
            configMapKeyRef:
              name: backend-config
              key: DB_SERVER
        - name: DB_NAME
          valueFrom:
            configMapKeyRef:
              name: backend-config
              key: DB_NAME
        - name: DB_USER
          value: "sa"
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: mssql-secret
              key: SA_PASSWORD
        - name: DB_INTEGRATED_SECURITY
          value: "false"
        - name: BLL_TYPE
          valueFrom:
            configMapKeyRef:
              name: backend-config
              key: BLL_TYPE
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /api/servicecategories
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /api/servicecategories
            port: 5000
          initialDelaySeconds: 20
          periodSeconds: 5

---

apiVersion: v1
kind: Service
metadata:
  name: backend-service
  namespace: serviceconnect
spec:
  selector:
    app: backend
  ports:
  - protocol: TCP
    port: 5000
    targetPort: 5000
  type: LoadBalancer
```

#### 2.4 Frontend Deployment

**File**: `k8s/frontend-deployment.yaml`

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: frontend
  namespace: serviceconnect
spec:
  replicas: 2
  selector:
    matchLabels:
      app: frontend
  template:
    metadata:
      labels:
        app: frontend
    spec:
      containers:
      - name: frontend
        image: <your-registry>/serviceconnect-ui:latest
        ports:
        - containerPort: 3000
        env:
        - name: NEXT_PUBLIC_API_URL
          value: "http://backend-service:5000/api"
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"

---

apiVersion: v1
kind: Service
metadata:
  name: frontend-service
  namespace: serviceconnect
spec:
  selector:
    app: frontend
  ports:
  - protocol: TCP
    port: 80
    targetPort: 3000
  type: LoadBalancer
```

#### 2.5 Horizontal Pod Autoscaler

**File**: `k8s/autoscaling.yaml`

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: backend-hpa
  namespace: serviceconnect
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: backend
  minReplicas: 2
  maxReplicas: 5
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80

---

apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: frontend-hpa
  namespace: serviceconnect
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: frontend
  minReplicas: 1
  maxReplicas: 3
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

---

## Deployment Commands

### Build Docker Images
```bash
# Backend
cd backend
docker build -t <your-registry>/serviceconnect-api:latest .
docker push <your-registry>/serviceconnect-api:latest

# Frontend
cd frontend/serviceconnect-ui
docker build -t <your-registry>/serviceconnect-ui:latest .
docker push <your-registry>/serviceconnect-ui:latest
```

### Deploy to Kubernetes
```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Deploy database
kubectl apply -f k8s/database-statefulset.yaml

# Wait for database to be ready
kubectl wait --for=condition=ready pod -l app=mssql -n serviceconnect --timeout=300s

# Deploy backend
kubectl apply -f k8s/backend-deployment.yaml

# Deploy frontend
kubectl apply -f k8s/frontend-deployment.yaml

# Configure autoscaling
kubectl apply -f k8s/autoscaling.yaml

# Check status
kubectl get all -n serviceconnect
```

---

## Additional Phase 4 Features to Implement

### 1. Database Initialization Job
Create a Kubernetes Job to run SQL initialization scripts on first deployment.

### 2. Monitoring with Prometheus
Add Prometheus metrics scraping for API endpoints.

### 3. Logging with ELK Stack
Centralized logging for all pods.

### 4. CI/CD Pipeline
GitHub Actions or Azure DevOps pipeline for automated deployments.

### 5. Helm Charts
Package all Kubernetes resources as Helm charts for easy deployment.

---

## Testing the Deployment

```bash
# Get external IPs
kubectl get services -n serviceconnect

# Access frontend
curl http://<frontend-service-external-ip>

# Access backend API
curl http://<backend-service-external-ip>/api/servicecategories

# Check pod logs
kubectl logs -f -l app=backend -n serviceconnect

# Scale manually
kubectl scale deployment backend --replicas=5 -n serviceconnect
```

---

## Next Steps for Phase 4

1. Set up local Kubernetes cluster (minikube, kind, or Docker Desktop)
2. Test Docker compose setup locally
3. Build and push Docker images
4. Deploy to Kubernetes
5. Configure monitoring and logging
6. Load test with autoscaling
7. Document Kubernetes architecture
8. Create presentation/demo

---

## Resources

- **Kubernetes Documentation**: https://kubernetes.io/docs/
- **Docker Documentation**: https://docs.docker.com/
- **SQL Server on Kubernetes**: https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-kubernetes
- **ASP.NET Core in Containers**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/
- **Next.js Docker**: https://nextjs.org/docs/deployment#docker-image

---
