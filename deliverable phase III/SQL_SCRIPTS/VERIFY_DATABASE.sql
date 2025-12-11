-- ========================================================================
-- DATABASE VERIFICATION SCRIPT
-- Run this after executing ServiceConnect_Phase2.sql
-- ========================================================================

USE ServiceConnect;
GO

PRINT '========================================================';
PRINT 'VERIFYING DATABASE SETUP';
PRINT '========================================================';
PRINT '';

-- Check row counts
PRINT '=== TABLE ROW COUNTS ===';
SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM Users
UNION ALL SELECT 'Workers', COUNT(*) FROM Workers
UNION ALL SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL SELECT 'ServiceCategories', COUNT(*) FROM ServiceCategories
UNION ALL SELECT 'WorkerSkills', COUNT(*) FROM WorkerSkills
UNION ALL SELECT 'Jobs', COUNT(*) FROM Jobs
UNION ALL SELECT 'Bids', COUNT(*) FROM Bids
UNION ALL SELECT 'Bookings', COUNT(*) FROM Bookings
UNION ALL SELECT 'Reviews', COUNT(*) FROM Reviews
UNION ALL SELECT 'Notifications', COUNT(*) FROM Notifications
ORDER BY TableName;

PRINT '';
PRINT '=== STORED PROCEDURES ===';
SELECT name AS ProcedureName FROM sys.objects WHERE type = 'P' AND is_ms_shipped = 0 ORDER BY name;

PRINT '';
PRINT '=== FUNCTIONS ===';
SELECT name AS FunctionName, type_desc FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0 ORDER BY name;

PRINT '';
PRINT '=== TRIGGERS ===';
SELECT t.name AS TriggerName, OBJECT_NAME(t.parent_object_id) AS TableName, t.type_desc
FROM sys.triggers t
WHERE t.is_ms_shipped = 0
ORDER BY t.name;

PRINT '';
PRINT '=== VIEWS ===';
SELECT name AS ViewName FROM sys.views WHERE is_ms_shipped = 0 ORDER BY name;

PRINT '';
PRINT '=== INDEXES (Non-PK, Non-Unique Constraints) ===';
SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE i.is_primary_key = 0
    AND i.is_unique_constraint = 0
    AND i.object_id IN (SELECT object_id FROM sys.objects WHERE type = 'U' AND is_ms_shipped = 0)
ORDER BY TableName, IndexName;

PRINT '';
PRINT '========================================================';
PRINT 'VERIFICATION COMPLETE';
PRINT '========================================================';
PRINT '';
PRINT 'Expected Row Counts:';
PRINT '  Users: 50,000';
PRINT '  Workers: 25,000';
PRINT '  Customers: 25,000';
PRINT '  ServiceCategories: 8';
PRINT '  WorkerSkills: 250,000';
PRINT '  Jobs: 400,000';
PRINT '  Bids: 500,000';
PRINT '  Bookings: 150,000+';
PRINT '';
PRINT 'Expected Features:';
PRINT '  Stored Procedures: 6';
PRINT '  Functions: 4';
PRINT '  Triggers: 5';
PRINT '  Views: 4';
PRINT '  Indexes: 15+';
GO
