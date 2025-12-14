# SQL Server High Availability with Kubernetes - Complete Guide

## What This Demonstrates

This setup shows **complete high availability** for SQL Server using Kubernetes:
- ✅ **2 SQL Server replicas** with identical data
- ✅ **Automatic failover** when pods crash
- ✅ **Zero downtime** during failures
- ✅ **Self-healing** pods that automatically recover
- ✅ **Data persistence** across pod restarts
- ✅ **Load balancing** across healthy pods

Your **backend and frontend run locally**, connecting to Kubernetes SQL Server.

---

## Architecture

```
Your Machine:
├── Frontend (Next.js) → :3000
│   └→ Backend (.NET) → :5000
│       └→ localhost:31433 (Kubernetes NodePort)
│
Kubernetes (Docker Desktop):
├── sqlserver-0 (10Gi storage, 1.3M rows)
└── sqlserver-1 (10Gi storage, 1.3M rows - replicated)
    └→ NodePort Service :31433
        - Routes to healthy pods automatically
        - Session affinity enabled
```

---

## Complete Setup (From Scratch)

### Prerequisites
- Docker Desktop with Kubernetes enabled
- kubectl installed
- 8GB+ RAM allocated to Docker

### Step 1: Deploy SQL Server to Kubernetes

```powershell
cd kubernetes-sqlserver
.\deploy.ps1
```

**Wait 2-3 minutes** for both pods to be ready:
```powershell
kubectl get pods -n sqlserver-ha -w
```

Expected:
```
sqlserver-0   1/1     Running
sqlserver-1   1/1     Running
```

### Step 2: Initialize Database on sqlserver-0

```powershell
.\init-database.ps1
```

**Takes 5-10 minutes** to load 1.3M+ rows.

### Step 3: Replicate Data to sqlserver-1

```powershell
.\replicate-data.ps1
```

**Takes 2-3 minutes** to backup and restore database.

Now **both pods have identical data**!

### Step 4: Update Backend Configuration

Edit: `deliverable phase III\backend\.env`

Change:
```env
DB_SERVER=localhost
```

To:
```env
DB_SERVER=localhost,31433
```

### if u face errors try setting all envs in backend to
DB_SERVER="127.0.0.1,31433"

# run in ps for portforwarding:
```powershell
while ($true) {
  kubectl port-forward -n sqlserver-ha svc/sqlserver 31433:1433
  Start-Sleep -Seconds 2  # small backoff before retry
}
```
## then run backend

**That's it!** Your backend now connects to Kubernetes SQL Server.

### Step 5: Run Your Application
## Run following command to test backend api
Invoke-WebRequest -Uri "http://localhost:5000/api/servicecategories" | Select-Object StatusCode, Content

# or run
Invoke-RestMethod -Uri "http://localhost:5000/api/stats/counts"
**Terminal 1 - Backend (EASIEST WAY):**
```powershell
cd "deliverable phase III\backend"
.\start-backend.ps1
```

This script automatically sets all environment variables and starts the backend.

**Expected Output:**
```
Database: ServiceConnect on localhost,31433
```

**Alternative - Manual Start:**

**Terminal 1 - Backend:**
```powershell
cd "deliverable phase III\backend"
$env:DB_SERVER="localhost,31433"
$env:DOTNET_ROLL_FORWARD="Major"
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ASPNETCORE_URLS="https://localhost:5001;http://localhost:5000"
dotnet run --project ServiceConnect.API
```

**Terminal 2 - Frontend:**
```powershell
cd "deliverable phase III\frontend\serviceconnect-ui"
npm run dev
```

**Access:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000/api
- Swagger: http://localhost:5000/swagger

---

## Testing High Availability

### Comprehensive Failover Test (Recommended!)

This test demonstrates **complete HA** with zero downtime:

```powershell
cd kubernetes-sqlserver
.\comprehensive-failover-test.ps1
```

**What it does:**
1. ✅ Verifies both pods are running with data
2. ✅ Deletes sqlserver-0 → Traffic routes to sqlserver-1
3. ✅ **Your app keeps working** (zero downtime!)
4. ✅ Waits for sqlserver-0 to auto-recover
5. ✅ Deletes sqlserver-1 → Traffic routes to sqlserver-0
6. ✅ **Your app STILL works** (zero downtime!)
7. ✅ Waits for sqlserver-1 to auto-recover
8. ✅ Both pods back online with all data intact

**Make sure your backend is running before starting the test!**

### Quick Single Pod Failover
# NOTE WHEN DELETING BOTH PODS THE PORTFORWARDING NEEDS TO BE DONE AGAIN
portforwarding:
# run in ps
while ($true) {
  kubectl port-forward -n sqlserver-ha svc/sqlserver 31433:1433
  Start-Sleep -Seconds 2  # small backoff before retry
}
Test one pod failure:

```powershell
# Delete a pod
kubectl delete pod -n sqlserver-ha sqlserver-0

# Watch automatic recovery
kubectl get pods -n sqlserver-ha -w

# Your app continues working throughout!
```

---

## Simulating Failures
# NOTE WHEN DELETING BOTH PODS THE PORTFORWARDING NEEDS TO BE DONE AGAIN
### Scenario 1: Pod Crash
```powershell
# Simulate pod crash
kubectl delete pod -n sqlserver-ha sqlserver-0

# Kubernetes recreates it automatically
# App continues using sqlserver-1
```

### Scenario 2: Both Pods Crash (One at a Time) NOTE WHEN DELETING BOTH PODS THE PORTFORWARDING NEEDS TO BE DONE AGAIN
```powershell
# Delete first pod
kubectl delete pod -n sqlserver-ha sqlserver-0

# Wait for recovery
kubectl wait --for=condition=ready pod/sqlserver-0 -n sqlserver-ha

# Delete second pod
kubectl delete pod -n sqlserver-ha sqlserver-1

# App switches between pods seamlessly!
```

### Scenario 3: Verify No Data Loss
```powershell
# Before failure - check row count
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT COUNT(*) FROM Users"

# Delete the pod
kubectl delete pod -n sqlserver-ha sqlserver-0

# Wait for recovery
kubectl wait --for=condition=ready pod/sqlserver-0 -n sqlserver-ha

# After recovery - check row count (should be same!)
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT COUNT(*) FROM Users"
```

---

## Monitoring & Troubleshooting

### Check Pod Status
```powershell
# View all pods
kubectl get pods -n sqlserver-ha

# Watch for changes
kubectl get pods -n sqlserver-ha -w

# Detailed pod info
kubectl describe pod -n sqlserver-ha sqlserver-0
```

### View Logs
```powershell
# Pod logs
kubectl logs -n sqlserver-ha sqlserver-0

# Follow logs
kubectl logs -n sqlserver-ha sqlserver-0 -f

# Last 50 lines
kubectl logs -n sqlserver-ha sqlserver-0 --tail=50
```

### Test SQL Connectivity
```powershell
# Test sqlserver-0
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT @@VERSION"

# Test sqlserver-1
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "SELECT @@VERSION"

# Test from your machine
# (if you have sqlcmd installed locally)
sqlcmd -S localhost,31433 -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION"
```

### Check Resource Usage
```powershell
# Pod resources
kubectl top pods -n sqlserver-ha

# Node resources
kubectl top nodes
```

### Verify Data Replication
```powershell
# Check row counts on sqlserver-0
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT 'sqlserver-0' AS Pod, (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) AS TotalRows"

# Check row counts on sqlserver-1
kubectl exec -n sqlserver-ha sqlserver-1 -- /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE ServiceConnect; SELECT 'sqlserver-1' AS Pod, (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) AS TotalRows"
```

---

## Cleanup

### Remove SQL Server Only (Keep Kubernetes Running)

**Use this when you're done and want to free up disk space:**

```powershell
# Delete all SQL Server resources (pods, data, services)
.\cleanup.ps1

# This removes:
# - All SQL Server pods (sqlserver-0, sqlserver-1)
# - All persistent volumes (~20GB freed!)
# - All services and configs
# - The entire sqlserver-ha namespace
```

**Kubernetes stays running** and you can deploy again later with `.\deploy.ps1`

### Stop Kubernetes Completely (Optional)

**To fully stop Kubernetes and free ALL resources:**

1. **Docker Desktop → Settings → Kubernetes**
2. **Uncheck "Enable Kubernetes"**
3. **Click "Apply & Restart"**

This stops all Kubernetes resources and frees all disk space.

**To re-enable:** Check "Enable Kubernetes" again and wait for it to restart.

### Redeploy SQL Server Later

```powershell
# When you want to use it again
.\deploy.ps1
.\init-database.ps1
.\replicate-data.ps1
```

---

## Files in This Directory

| File | Purpose |
|------|---------|
| `deploy.ps1` | Deploy SQL Server to Kubernetes |
| `init-database.ps1` | Load database schema and data to sqlserver-0 |
| `replicate-data.ps1` | Copy data from sqlserver-0 to sqlserver-1 |
| `comprehensive-failover-test.ps1` | **Full HA test with zero downtime demo** |
| `test-failover.ps1` | Quick single pod failover test |
| `cleanup.ps1` | Remove all Kubernetes resources |
| `namespace.yaml` | Kubernetes namespace definition |
| `secret.yaml` | SQL Server password |
| `sqlserver-statefulset.yaml` | 2 SQL Server replicas |
| `sqlserver-service.yaml` | NodePort service (port 31433) |
| `poddisruptionbudget.yaml` | High availability policy |

---

## What This Demonstrates

### Kubernetes Features
✅ **StatefulSets** - Stable pod identities and persistent storage
✅ **Persistent Volumes** - Data survives pod restarts
✅ **Services** - Load balancing and service discovery
✅ **NodePort** - External access to cluster services
✅ **Health Checks** - Liveness and readiness probes
✅ **Self-Healing** - Automatic pod recreation on failure
✅ **PodDisruptionBudgets** - Guaranteed minimum availability

### High Availability Features
✅ **Data Replication** - Both pods have identical data
✅ **Automatic Failover** - Traffic routes to healthy pods
✅ **Zero Downtime** - Application continues during failures
✅ **Data Persistence** - No data loss during pod restarts
✅ **Load Balancing** - Requests distributed across pods
✅ **Session Affinity** - Sticky sessions for SQL connections

---

## Quick Reference

### Deploy Everything
```powershell
.\deploy.ps1
.\init-database.ps1
.\replicate-data.ps1
```

### Update Backend
```env
DB_SERVER=localhost,31433
```

### Test HA
```powershell
.\comprehensive-failover-test.ps1
```

### Monitor
```powershell
kubectl get pods -n sqlserver-ha -w
```

### Clean Up (Free Disk Space)
```powershell
# Remove SQL Server (keeps Kubernetes running)
.\cleanup.ps1

# Stop Kubernetes completely (Docker Desktop → Settings → Kubernetes → Uncheck)
```

### Redeploy
```powershell
.\deploy.ps1
.\init-database.ps1
.\replicate-data.ps1
```

---

## Troubleshooting

### Pods Won't Start
```powershell
kubectl describe pod -n sqlserver-ha sqlserver-0
kubectl get events -n sqlserver-ha --sort-by='.lastTimestamp'
```

### Backend Can't Connect
- Verify pods are running: `kubectl get pods -n sqlserver-ha`
- Check service: `kubectl get svc -n sqlserver-ha`
- Verify .env: `DB_SERVER=localhost,31433` (note the comma!)

### Data Not Replicated
```powershell
.\replicate-data.ps1
```

### Pod Stuck in ContainerCreating
Wait 2-3 minutes. SQL Server image is large (~1.5GB).

---

## Success Criteria

Your setup is working when:
✅ Both pods show `1/1 Running`
✅ Backend connects to `localhost:31433`
✅ Frontend loads at http://localhost:3000
✅ Deleting a pod → App keeps working
✅ Pod auto-recovers within 60-90 seconds
✅ No data loss after pod restart
