#!/bin/bash
# Bash script to cleanup SQL Server Kubernetes resources

echo "========================================"
echo "sql server kubernetes cleanup"
echo "==================================="
echo ""

echo "note: this deletes all sql server resources from ubernetes"
echo "including"
echo "  all sql pods"
echo "  all persistent volumes (db data gone)"
echo "  all services"
echo "  all secrets"
echo "  entire 'sqlserver-ha' namespace"
echo ""

read -p "are you sure you want to continue? Type 'yes' to confirm: " confirm

if [ "$confirm" != "yes" ]; then
    echo "cleanup cancelled"
    exit 0
fi

echo ""
echo "deleting sqlserver-ha namespace"
kubectl delete namespace sqlserver-ha

echo ""
echo "wiat for namespace deletion to complete..."
kubectl wait --for=delete namespace/sqlserver-ha --timeout=120s

echo ""
echo "cleanup done"
echo "========================================"
echo ""

echo "sql server resources have been removed"
echo ""

echo "to deploy again"
echo "  ./deploy.sh"
echo ""
