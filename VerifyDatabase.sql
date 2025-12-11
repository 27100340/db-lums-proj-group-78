USE ServiceConnect;
GO

PRINT '========================================';
PRINT 'SERVICECONNECT DATABASE VERIFICATION';
PRINT '========================================';
PRINT '';

PRINT 'Data Row Counts:';
PRINT 'Total Users: ' + CAST((SELECT COUNT(*) FROM Users) AS VARCHAR);
PRINT 'Total Workers: ' + CAST((SELECT COUNT(*) FROM Workers) AS VARCHAR);
PRINT 'Total Customers: ' + CAST((SELECT COUNT(*) FROM Customers) AS VARCHAR);
PRINT 'Total ServiceCategories: ' + CAST((SELECT COUNT(*) FROM ServiceCategories) AS VARCHAR);
PRINT 'Total WorkerSkills: ' + CAST((SELECT COUNT(*) FROM WorkerSkills) AS VARCHAR);
PRINT 'Total Jobs: ' + CAST((SELECT COUNT(*) FROM Jobs) AS VARCHAR);
PRINT 'Total Bids: ' + CAST((SELECT COUNT(*) FROM Bids) AS VARCHAR);
PRINT 'Total Bookings: ' + CAST((SELECT COUNT(*) FROM Bookings) AS VARCHAR);
PRINT 'Total Reviews: ' + CAST((SELECT COUNT(*) FROM Reviews) AS VARCHAR);
PRINT 'Total Notifications: ' + CAST((SELECT COUNT(*) FROM Notifications) AS VARCHAR);
PRINT 'Total WorkerAvailability: ' + CAST((SELECT COUNT(*) FROM WorkerAvailability) AS VARCHAR);
PRINT 'Total WorkerServiceAreas: ' + CAST((SELECT COUNT(*) FROM WorkerServiceAreas) AS VARCHAR);
PRINT 'Total JobAttachments: ' + CAST((SELECT COUNT(*) FROM JobAttachments) AS VARCHAR);
PRINT '';

DECLARE @TotalRows INT;
SELECT @TotalRows = (SELECT COUNT(*) FROM Users) + (SELECT COUNT(*) FROM Workers) + (SELECT COUNT(*) FROM Customers) + (SELECT COUNT(*) FROM ServiceCategories) + (SELECT COUNT(*) FROM WorkerSkills) + (SELECT COUNT(*) FROM Jobs) + (SELECT COUNT(*) FROM Bids) + (SELECT COUNT(*) FROM Bookings) + (SELECT COUNT(*) FROM Reviews) + (SELECT COUNT(*) FROM Notifications) + (SELECT COUNT(*) FROM WorkerAvailability) + (SELECT COUNT(*) FROM WorkerServiceAreas) + (SELECT COUNT(*) FROM JobAttachments);

PRINT 'Grand Total Rows: ' + CAST(@TotalRows AS VARCHAR);
PRINT '';

PRINT '========================================';
PRINT 'Database Objects:';
PRINT '========================================';
PRINT '';

PRINT 'Stored Procedures: ' + CAST((SELECT COUNT(*) FROM sys.objects WHERE type = 'P' AND name LIKE 'sp_%') AS VARCHAR);
PRINT 'Functions: ' + CAST((SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN', 'IF', 'TF')) AS VARCHAR);
PRINT 'Views: ' + CAST((SELECT COUNT(*) FROM sys.objects WHERE type = 'V' AND name LIKE 'vw_%') AS VARCHAR);
PRINT 'Triggers: ' + CAST((SELECT COUNT(*) FROM sys.objects WHERE type = 'TR') AS VARCHAR);
PRINT 'Indexes: ' + CAST((SELECT COUNT(*) FROM sys.indexes WHERE object_id IN (SELECT object_id FROM sys.tables WHERE name IN ('Users', 'Workers', 'Customers', 'Jobs', 'Bids', 'Bookings', 'Reviews', 'WorkerSkills')) AND index_id > 0) AS VARCHAR);
PRINT '';

PRINT '========================================';
PRINT 'Verification Complete!';
PRINT '========================================';
GO
