#!/bin/bash
# Bash script to initialize the database in Kubernetes SQL Server

set -e

echo "========================================"
echo "Database Initialization"
echo "========================================"
echo ""

SQL_SCRIPT_PATH="../ServiceConnect_Phase2_CORRECTED.sql"

# Check if SQL script exists
if [ ! -f "$SQL_SCRIPT_PATH" ]; then
    echo "Error: SQL script not found at $SQL_SCRIPT_PATH"
    echo "Please ensure ServiceConnect_Phase2_CORRECTED.sql exists in the project root."
    exit 1
fi

echo "Found SQL script: $SQL_SCRIPT_PATH"
echo ""

echo "[1/3] Copying SQL script to sqlserver-0 pod..."
kubectl cp "$SQL_SCRIPT_PATH" sqlserver-ha/sqlserver-0:/tmp/init.sql
echo "SQL script copied!"
echo ""

echo "[2/3] Executing SQL script on sqlserver-0..."
echo "This may take several minutes (1.3M+ rows to insert)..."
kubectl exec -it -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -i /tmp/init.sql

echo ""
echo "Database initialized on sqlserver-0!"
echo ""

echo "[3/3] Verifying database..."
VERIFY_QUERY="USE ServiceConnect; SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';"
kubectl exec -n sqlserver-ha sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "$VERIFY_QUERY"

echo ""
echo "========================================"
echo "Database Initialization Complete!"
echo "========================================"
echo ""

echo "Database is ready on:"
echo "  - sqlserver-0 (Primary)"
echo "  - sqlserver-1 (Secondary - for failover)"
echo ""

echo "Note: sqlserver-1 is a separate instance (not replicated)."
echo "For true data replication, you would need to configure:"
echo "  - SQL Server Always On Availability Groups"
echo "  - Or manual database backup/restore to sqlserver-1"
echo ""

echo "Next step:"
echo "  Update backend .env: DB_SERVER=localhost,31433"
echo ""
