#!/bin/bash
# bash script to deploy sql server to Kubernetes with high availability

set -e

echo "========================================"
echo "sql server high availabilty dpeloyment"
echo "========================================"
echo ""

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "fault: kubectl not found"
    exit 1
fi

echo "making namespace"
kubectl apply -f namespace.yaml
echo ""

echo "making secret"
kubectl apply -f secret.yaml
echo ""

echo "making sql server stateful replicas(2)"
kubectl apply -f sqlserver-statefulset.yaml

echo ""

echo "making sql server services"
kubectl apply -f sqlserver-service.yaml

echo ""

echo " making poddisruptionbudget"
kubectl apply -f poddisruptionbudget.yaml
echo ""

echo "waiting for sql pods to initialize"
echo "this takes time"
kubectl wait --for=condition=ready pod -l app=sqlserver -n sqlserver-ha --timeout=300s

echo ""
echo "deploying done"
echo "========================================"
echo ""

echo "sql server pods:"
kubectl get pods -n sqlserver-ha
echo ""

echo "connection info"
echo "  Host: localhost"
echo "  Port: 31433"
echo "  User: sa"
echo "  Password: YourStrong!Passw0rd"
echo ""

echo "change backend .env file with:"
echo "  DB_SERVER=localhost,31433"
echo "  DB_USER=sa"
echo "  DB_PASSWORD=YourStrong!Passw0rd"
echo ""

echo "testing the connection:"
echo "  kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -Q 'SELECT @@VERSION'"
echo ""

echo "high available features"
echo "  2 sql server replicas running"
echo "  each has10Gi persistent storage"
echo "  auto pod recovery on failure"
echo "  distibutionbudget ensures 1 pod always available"
echo "  health checkign for automatic restart"
echo ""

echo "now do"
echo "  startup db using ./init-database.sh"
echo "  modify backend .env with the connection info above"
echo "  test the failover using ./test-failover.sh"
echo ""
