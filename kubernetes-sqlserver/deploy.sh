#!/bin/bash
# Bash script to deploy SQL Server to Kubernetes with High Availability

set -e

echo "========================================"
echo "SQL Server High Availability Deployment"
echo "========================================"
echo ""

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "Error: kubectl not found. Please install kubectl first."
    exit 1
fi

echo "[1/5] Creating namespace..."
kubectl apply -f namespace.yaml
echo "Namespace created!"
echo ""

echo "[2/5] Creating secret..."
kubectl apply -f secret.yaml
echo "Secret created!"
echo ""

echo "[3/5] Deploying SQL Server StatefulSet (2 replicas)..."
kubectl apply -f sqlserver-statefulset.yaml
echo "SQL Server StatefulSet created!"
echo ""

echo "[4/5] Creating SQL Server services..."
kubectl apply -f sqlserver-service.yaml
echo "Services created!"
echo ""

echo "[5/5] Creating PodDisruptionBudget..."
kubectl apply -f poddisruptionbudget.yaml
echo "PodDisruptionBudget created!"
echo ""

echo "Waiting for SQL Server pods to be ready..."
echo "This may take 2-3 minutes..."
kubectl wait --for=condition=ready pod -l app=sqlserver -n sqlserver-ha --timeout=300s

echo ""
echo "========================================"
echo "Deployment Complete!"
echo "========================================"
echo ""

echo "SQL Server Status:"
kubectl get pods -n sqlserver-ha
echo ""

echo "Connection Information:"
echo "  Host: localhost"
echo "  Port: 31433"
echo "  User: sa"
echo "  Password: YourStrong!Passw0rd"
echo ""

echo "Update your backend .env file with:"
echo "  DB_SERVER=localhost,31433"
echo "  DB_USER=sa"
echo "  DB_PASSWORD=YourStrong!Passw0rd"
echo ""

echo "Test connection:"
echo "  kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -Q 'SELECT @@VERSION'"
echo ""

echo "High Availability Features:"
echo "  - 2 SQL Server replicas running"
echo "  - Each with 10Gi persistent storage"
echo "  - Automatic pod recovery on failure"
echo "  - PodDisruptionBudget ensures 1 pod always available"
echo "  - Health checks for automatic restart"
echo ""

echo "Next steps:"
echo "  1. Initialize database: ./init-database.sh"
echo "  2. Update backend .env with the connection info above"
echo "  3. Test failover: ./test-failover.sh"
echo ""
