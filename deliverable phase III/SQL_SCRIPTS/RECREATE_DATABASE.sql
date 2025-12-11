-- ========================================================================
-- PHASE 3: DATABASE RECREATION SCRIPT
-- Run this script in SQL Server Management Studio (SSMS)
-- ========================================================================

-- STEP 1: Drop existing database (if exists)
PRINT '========================================================';
PRINT 'STEP 1: Dropping existing ServiceConnect database...';
PRINT '========================================================';
GO

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ServiceConnect')
BEGIN
    PRINT 'Found existing ServiceConnect database. Dropping...';
    ALTER DATABASE ServiceConnect SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ServiceConnect;
    PRINT 'ServiceConnect database dropped successfully!';
END
ELSE
BEGIN
    PRINT 'No existing ServiceConnect database found.';
END
GO

PRINT '';
PRINT '========================================================';
PRINT 'STEP 2: Now run the ServiceConnect_Phase2.sql script';
PRINT '========================================================';
PRINT 'Open ServiceConnect_Phase2.sql and execute it completely.';
PRINT 'This will create the database with 1.3M+ rows.';
PRINT '';
PRINT 'After completion, verify with:';
PRINT 'USE ServiceConnect;';
PRINT 'SELECT COUNT(*) FROM Users; -- Should be 50,000';
PRINT 'SELECT COUNT(*) FROM Jobs; -- Should be 400,000';
PRINT 'SELECT COUNT(*) FROM Bids; -- Should be 500,000';
GO
