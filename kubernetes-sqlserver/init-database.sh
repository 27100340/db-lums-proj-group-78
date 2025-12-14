#!/bin/bash
# Bash script to initialize the database in Kubernetes SQL Server

set -e

echo "================================="
echo "db init"
echo "========================================"
echo ""

SQL_SCRIPT_PATH="../ServiceConnect_Phase2_CORRECTED.sql"

# Check if SQL script exists
if [ ! -f "$SQL_SCRIPT_PATH" ]; then
    echo "fault sql script not found at $SQL_SCRIPT_PATH"
    echo "ensure ServiceConnect_Phase2_CORRECTED.sql is there in the project root"
    exit 1
fi

echo "sql script found $SQL_SCRIPT_PATH"
echo ""

echo "copying SQL script to sqlserver-0 pod"
kubectl cp "$SQL_SCRIPT_PATH" sqlserver-ha/sqlserver-0:/tmp/init.sql
echo ""

echo "executing SQL script on sqlserver-0"
echo "this step takes some time"
kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -i /tmp/init.sql

echo ""
echo "db initialized on sqlserver-0"
echo ""

echo "checking database"
VERIFY_QUERY="USE ServiceConnect; SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "$VERIFY_QUERY"

echo ""
echo "===================================="
echo "db init done"
echo "========================================"
echo ""

echo "db ready on:"
echo "  sqlserver-0 as primary"
echo "  sqlserver-1 as secondary for failover"
echo ""

echo "note: sqlserver-1 is a separate instance it isnt replicated"
echo "for actual replication you need to configure"
echo "  sql server always on availability groups"
echo "  or use manual database backup/restore to sqlserver-1"
echo ""

echo "next step"
echo "  update backend .env: DB_SERVER=localhost,31433"
echo ""
