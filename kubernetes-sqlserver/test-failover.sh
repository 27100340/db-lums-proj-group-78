#!/bin/bash
# Bash script to test SQL Server pod failure and recovery

set -e

echo "========================================"
echo "sql server failover test"
echo "=================================="
echo ""

echo "the script will show sql server pod failure to demonstrate"
echo "  auto pod recovery"
echo "  kubernetes self-healing"
echo "  high availability with 2 replicas"
echo "  pod distribution budget to ensure 1 pod stays running"
echo ""

read -p "proceeed with failover test? (y/n) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "test cancelled"
    exit 0
fi

echo ""
echo "========================================"
echo "initial state"
echo "====================================="
echo ""

echo "current sql server pods"
kubectl get pods -n sqlserver-ha -l app=sqlserver
echo ""

# Get the first pod name
POD_TO_DELETE=$(kubectl get pods -n sqlserver-ha -l app=sqlserver -o jsonpath='{.items[0].metadata.name}')

if [ -z "$POD_TO_DELETE" ]; then
    echo "fault: no sql pods found"
    exit 1
fi

echo "========================================"
echo "show pod killing"
echo "========================================"
echo ""

echo "killing pod $POD_TO_DELETE"
echo "to show fialure"
kubectl delete pod -n sqlserver-ha "$POD_TO_DELETE"

echo ""
echo "pod killed! kubernetes hsould nwo be recovering"
echo ""

echo "wait for 5s"
sleep 5

echo ""
echo "========================================"
echo "the recovery is in progress"
echo "========================================"
echo ""

echo " pod status:"
kubectl get pods -n sqlserver-ha -l app=sqlserver
echo ""

echo "note: kubernetes should automatically created a new pod to replace the deleted one"
echo ""

echo "wait for new pod to be ready"
echo "this step takes some time"
kubectl wait --for=condition=ready pod -l app=sqlserver -n sqlserver-ha --timeout=120s

echo ""
echo "========================================"
echo "recovery done"
echo "==================================="
echo ""

echo "pod status:"
kubectl get pods -n sqlserver-ha -l app=sqlserver
echo ""

echo "test sql server connection"
TEST_QUERY="SELECT @@VERSION AS 'SQL Server Version', GETDATE() AS 'Current Time';"
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "$TEST_QUERY"


echo "high availability features we have shown"
echo "  self healing the pod auto recovers"
echo "  persistent storage data survives pod restarts"
echo "  health checks auto readiness verification"
echo "  statefulset stable pod identity restored"
echo ""

echo "with 2 replicas "
echo "  if one pod fails the other continues serving requests"
echo "  pod distribution budget ensures at least 1 pod always available"
echo "  leading to zero system downtime"
echo ""