#!/bin/bash
# Bash script to cleanup SQL Server Kubernetes resources

echo "========================================"
echo "SQL Server Kubernetes Cleanup"
echo "========================================"
echo ""

echo "WARNING: This will delete all SQL Server resources from Kubernetes!"
echo "This includes:"
echo "  - All SQL Server pods"
echo "  - All persistent volumes (DATABASE DATA WILL BE LOST!)"
echo "  - All services"
echo "  - All secrets"
echo "  - The entire 'sqlserver-ha' namespace"
echo ""

read -p "Are you sure you want to continue? Type 'yes' to confirm: " confirm

if [ "$confirm" != "yes" ]; then
    echo "Cleanup cancelled."
    exit 0
fi

echo ""
echo "Deleting sqlserver-ha namespace..."
kubectl delete namespace sqlserver-ha

echo ""
echo "Waiting for namespace deletion to complete..."
kubectl wait --for=delete namespace/sqlserver-ha --timeout=120s

echo ""
echo "========================================"
echo "Cleanup Complete!"
echo "========================================"
echo ""

echo "All SQL Server resources have been removed."
echo ""

echo "To redeploy:"
echo "  ./deploy.sh"
echo ""
