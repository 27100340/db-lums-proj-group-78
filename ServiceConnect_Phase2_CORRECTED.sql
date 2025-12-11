-- ========================================================================
-- ServiceConnect Database - Phase 2 Implementation
-- Complete SQL Server Database with 1M+ Rows and Advanced Features
-- Group: Baqir Hassan (27100340)
-- CORRECTED VERSION - All errors fixed
-- ========================================================================

PRINT '========================================================';
PRINT 'ServiceConnect Database Setup - STARTING';
PRINT '========================================================';
GO

USE master;
GO

-- Drop existing database if it exists
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ServiceConnect')
BEGIN
    PRINT 'Dropping existing ServiceConnect database...';
    ALTER DATABASE ServiceConnect SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ServiceConnect;
    PRINT 'ServiceConnect database dropped successfully!';
END
GO

PRINT 'Creating new ServiceConnect database...';
CREATE DATABASE ServiceConnect;
GO

USE ServiceConnect;
GO

PRINT 'Switched to ServiceConnect database context.';
GO

-- ========================================================================
-- SECTION 1: CREATE TABLES
-- ========================================================================

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20),
    UserType VARCHAR(20) CHECK (UserType IN ('Customer', 'Worker')),
    RegistrationDate DATETIME DEFAULT GETDATE(),
    LastActive DATETIME,
    IsVerified BIT DEFAULT 0,
    AccountStatus VARCHAR(20) DEFAULT 'Active'
);

CREATE TABLE Workers (
    WorkerID INT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    DateOfBirth DATE,
    GovernmentID VARCHAR(50),
    Address VARCHAR(200),
    City VARCHAR(50),
    PostalCode VARCHAR(10),
    BackgroundCheckStatus VARCHAR(20),
    BackgroundCheckDate DATE,
    InsuranceNumber VARCHAR(50),
    ProfilePhotoURL VARCHAR(255),
    Bio VARCHAR(MAX),
    ExperienceYears INT,
    HourlyRate DECIMAL(10,2),
    OverallRating DECIMAL(3,2),
    TotalJobsCompleted INT DEFAULT 0,
    CONSTRAINT FK_Workers_Users FOREIGN KEY (WorkerID) REFERENCES Users(UserID)
);

CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Address VARCHAR(200),
    City VARCHAR(50),
    PostalCode VARCHAR(10),
    ProfilePhotoURL VARCHAR(255),
    CustomerRating DECIMAL(3,2),
    TotalJobsPosted INT DEFAULT 0,
    CONSTRAINT FK_Customers_Users FOREIGN KEY (CustomerID) REFERENCES Users(UserID)
);

CREATE TABLE ServiceCategories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(50) NOT NULL,
    CategoryDescription VARCHAR(MAX),
    IconURL VARCHAR(255),
    BaseRate DECIMAL(10,2),
    IsActive BIT DEFAULT 1
);

CREATE TABLE WorkerSkills (
    SkillID INT IDENTITY(1,1) PRIMARY KEY,
    WorkerID INT,
    CategoryID INT,
    SkillLevel VARCHAR(20) CHECK (SkillLevel IN ('Beginner', 'Intermediate', 'Expert')),
    CertificationURL VARCHAR(255),
    CertificationExpiry DATE,
    YearsExperience INT,
    CONSTRAINT FK_WorkerSkills_Workers FOREIGN KEY (WorkerID) REFERENCES Workers(WorkerID),
    CONSTRAINT FK_WorkerSkills_Categories FOREIGN KEY (CategoryID) REFERENCES ServiceCategories(CategoryID)
);

CREATE TABLE Jobs (
    JobID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT,
    CategoryID INT,
    Title VARCHAR(100) NOT NULL,
    Description VARCHAR(MAX),
    Budget DECIMAL(10,2),
    PostedDate DATETIME DEFAULT GETDATE(),
    StartDate DATETIME,
    EndDate DATETIME,
    Location VARCHAR(200),
    Latitude DECIMAL(10,7),
    Longitude DECIMAL(10,7),
    Status VARCHAR(20) DEFAULT 'Open' CHECK (Status IN ('Open', 'Assigned', 'InProgress', 'Completed', 'Cancelled')),
    UrgencyLevel VARCHAR(20) CHECK (UrgencyLevel IN ('Low', 'Medium', 'High', 'Urgent')),
    RequiredWorkers INT DEFAULT 1,
    CompletedWorkers INT DEFAULT 0,
    CONSTRAINT FK_Jobs_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Jobs_Categories FOREIGN KEY (CategoryID) REFERENCES ServiceCategories(CategoryID)
);

CREATE TABLE JobAttachments (
    AttachmentID INT IDENTITY(1,1) PRIMARY KEY,
    JobID INT,
    FileURL VARCHAR(255),
    FileType VARCHAR(50),
    UploadedDate DATETIME DEFAULT GETDATE(),
    Description VARCHAR(255),
    CONSTRAINT FK_JobAttachments_Jobs FOREIGN KEY (JobID) REFERENCES Jobs(JobID)
);

CREATE TABLE Bids (
    BidID INT IDENTITY(1,1) PRIMARY KEY,
    JobID INT,
    WorkerID INT,
    BidAmount DECIMAL(10,2),
    ProposedStartTime DATETIME,
    EstimatedDuration INT,
    CoverLetter VARCHAR(MAX),
    BidDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')),
    IsWinningBid BIT DEFAULT 0,
    CONSTRAINT FK_Bids_Jobs FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    CONSTRAINT FK_Bids_Workers FOREIGN KEY (WorkerID) REFERENCES Workers(WorkerID)
);

CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    JobID INT,
    WorkerID INT,
    BidID INT,
    ScheduledStart DATETIME,
    ScheduledEnd DATETIME,
    ActualStart DATETIME,
    ActualEnd DATETIME,
    Status VARCHAR(20) DEFAULT 'Scheduled' CHECK (Status IN ('Scheduled', 'InProgress', 'Completed', 'Cancelled')),
    CancellationReason VARCHAR(255),
    BookingCode VARCHAR(20) UNIQUE,
    CompletionNotes VARCHAR(MAX),
    CONSTRAINT FK_Bookings_Jobs FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    CONSTRAINT FK_Bookings_Workers FOREIGN KEY (WorkerID) REFERENCES Workers(WorkerID),
    CONSTRAINT FK_Bookings_Bids FOREIGN KEY (BidID) REFERENCES Bids(BidID)
);

CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID INT,
    ReviewerID INT,
    ReviewedID INT,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comment VARCHAR(MAX),
    ReviewDate DATETIME DEFAULT GETDATE(),
    IsDisputed BIT DEFAULT 0,
    DisputeResolution VARCHAR(MAX),
    WasHelpful INT DEFAULT 0,
    CONSTRAINT FK_Reviews_Bookings FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    CONSTRAINT FK_Reviews_Reviewer FOREIGN KEY (ReviewerID) REFERENCES Users(UserID),
    CONSTRAINT FK_Reviews_Reviewed FOREIGN KEY (ReviewedID) REFERENCES Users(UserID)
);

CREATE TABLE WorkerAvailability (
    AvailabilityID INT IDENTITY(1,1) PRIMARY KEY,
    WorkerID INT,
    DayOfWeek INT CHECK (DayOfWeek BETWEEN 1 AND 7),
    StartTime TIME,
    EndTime TIME,
    IsRecurring BIT DEFAULT 1,
    CONSTRAINT FK_WorkerAvailability_Workers FOREIGN KEY (WorkerID) REFERENCES Workers(WorkerID)
);

CREATE TABLE WorkerServiceAreas (
    ServiceAreaID INT IDENTITY(1,1) PRIMARY KEY,
    WorkerID INT,
    City VARCHAR(50),
    PostalCode VARCHAR(10),
    MaxDistance INT,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_WorkerServiceAreas_Workers FOREIGN KEY (WorkerID) REFERENCES Workers(WorkerID)
);

CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    NotificationType VARCHAR(50),
    Title VARCHAR(100),
    Message VARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsRead BIT DEFAULT 0,
    RelatedEntityID INT,
    RelatedEntityType VARCHAR(50),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

PRINT 'All tables created successfully!';
GO

-- ========================================================================
-- SECTION 2: CREATE INDEXES
-- ========================================================================

CREATE NONCLUSTERED INDEX IX_Jobs_CustomerID ON Jobs(CustomerID);
CREATE NONCLUSTERED INDEX IX_Jobs_CategoryID ON Jobs(CategoryID);
CREATE NONCLUSTERED INDEX IX_Jobs_Status ON Jobs(Status);
CREATE NONCLUSTERED INDEX IX_Jobs_PostedDate ON Jobs(PostedDate);
CREATE NONCLUSTERED INDEX IX_Bids_JobID ON Bids(JobID);
CREATE NONCLUSTERED INDEX IX_Bids_WorkerID ON Bids(WorkerID);
CREATE NONCLUSTERED INDEX IX_Bids_Status ON Bids(Status);
CREATE NONCLUSTERED INDEX IX_Bookings_JobID ON Bookings(JobID);
CREATE NONCLUSTERED INDEX IX_Bookings_WorkerID ON Bookings(WorkerID);
CREATE NONCLUSTERED INDEX IX_Bookings_Status ON Bookings(Status);
CREATE NONCLUSTERED INDEX IX_Reviews_BookingID ON Reviews(BookingID);
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Workers_City ON Workers(City);
CREATE NONCLUSTERED INDEX IX_WorkerSkills_WorkerID ON WorkerSkills(WorkerID);
CREATE NONCLUSTERED INDEX IX_Notifications_UserID ON Notifications(UserID);

CREATE NONCLUSTERED INDEX IX_WorkerSkills_Covering
ON WorkerSkills(WorkerID, CategoryID)
INCLUDE (SkillLevel, YearsExperience);

PRINT 'All indexes created successfully!';
GO

-- ========================================================================
-- SECTION 3: POPULATE DATA
-- ========================================================================

PRINT 'Starting data population...';
GO

-- Insert Service Categories
INSERT INTO ServiceCategories (CategoryName, CategoryDescription, BaseRate, IsActive)
VALUES
    ('Plumbing', 'Pipe installation and repair services', 50.00, 1),
    ('Electrical', 'Electrical installation and maintenance', 60.00, 1),
    ('Cleaning', 'Professional cleaning services', 30.00, 1),
    ('Carpentry', 'Wood work and furniture assembly', 55.00, 1),
    ('Painting', 'Interior and exterior painting', 40.00, 1),
    ('HVAC', 'Heating, ventilation, and air conditioning', 70.00, 1),
    ('Landscaping', 'Garden and outdoor maintenance', 35.00, 1),
    ('Handyman', 'General home repair and maintenance', 45.00, 1);

PRINT 'Service categories inserted (8 records).';
GO

-- Insert 50,000 Users
PRINT 'Inserting Users (50,000 records)...';
INSERT INTO Users (Email, PasswordHash, PhoneNumber, UserType, RegistrationDate, LastActive, IsVerified, AccountStatus)
SELECT TOP 50000
    'user' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR) + '@serviceconnect.com',
    'hash_' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR),
    '+92300' + RIGHT('000000' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR), 6),
    CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN 'Customer' ELSE 'Worker' END,
    DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 365), GETDATE()),
    DATEADD(MINUTE, -(ABS(CHECKSUM(NEWID())) % 10080), GETDATE()),
    1,
    'Active'
FROM master..spt_values a, master..spt_values b;

PRINT 'Users inserted (50,000 records).';
GO

-- Insert Workers (from odd UserIDs)
PRINT 'Inserting Workers (25,000 records)...';
INSERT INTO Workers (WorkerID, FirstName, LastName, DateOfBirth, GovernmentID, Address, City, PostalCode,
                      BackgroundCheckStatus, BackgroundCheckDate, InsuranceNumber, HourlyRate, OverallRating, TotalJobsCompleted, ExperienceYears)
SELECT
    UserID,
    'Worker' + CAST(UserID AS VARCHAR),
    'LastName' + CAST(UserID AS VARCHAR),
    DATEADD(YEAR, -(20 + (ABS(CHECKSUM(NEWID())) % 30)), GETDATE()),
    'GOV' + CAST(UserID AS VARCHAR),
    'Address ' + CAST(UserID AS VARCHAR),
    CASE WHEN UserID % 5 = 0 THEN 'Karachi'
         WHEN UserID % 5 = 1 THEN 'Lahore'
         WHEN UserID % 5 = 2 THEN 'Islamabad'
         WHEN UserID % 5 = 3 THEN 'Multan'
         ELSE 'Peshawar' END,
    'P' + CAST((ABS(CHECKSUM(NEWID())) % 99999) AS VARCHAR),
    'Passed',
    DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 365), GETDATE()),
    'INS' + CAST(UserID AS VARCHAR),
    CAST(30 + (ABS(CHECKSUM(NEWID())) % 100) AS DECIMAL(10,2)),
    CAST((ABS(CHECKSUM(NEWID())) % 500) / 100.0 AS DECIMAL(3,2)),
    (ABS(CHECKSUM(NEWID())) % 50),
    (ABS(CHECKSUM(NEWID())) % 20) + 1
FROM Users
WHERE UserType = 'Worker';

PRINT 'Workers inserted (25,000 records).';
GO

-- Insert Customers (from even UserIDs)
PRINT 'Inserting Customers (25,000 records)...';
INSERT INTO Customers (CustomerID, FirstName, LastName, Address, City, PostalCode, CustomerRating, TotalJobsPosted)
SELECT
    UserID,
    'Customer' + CAST(UserID AS VARCHAR),
    'LastName' + CAST(UserID AS VARCHAR),
    'Address ' + CAST(UserID AS VARCHAR),
    CASE WHEN UserID % 5 = 0 THEN 'Karachi'
         WHEN UserID % 5 = 1 THEN 'Lahore'
         WHEN UserID % 5 = 2 THEN 'Islamabad'
         WHEN UserID % 5 = 3 THEN 'Multan'
         ELSE 'Peshawar' END,
    'P' + CAST((ABS(CHECKSUM(NEWID())) % 99999) AS VARCHAR),
    CAST((ABS(CHECKSUM(NEWID())) % 500) / 100.0 AS DECIMAL(3,2)),
    (ABS(CHECKSUM(NEWID())) % 30)
FROM Users
WHERE UserType = 'Customer';

PRINT 'Customers inserted (25,000 records).';
GO

-- Insert Worker Skills (250,000 records) - Reference actual Worker IDs
PRINT 'Inserting Worker Skills (250,000 records)...';
WITH WorkerIDs AS (
    SELECT WorkerID, ROW_NUMBER() OVER (ORDER BY WorkerID) AS RowNum
    FROM Workers
),
NumberedRows AS (
    SELECT TOP 250000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM master..spt_values a CROSS JOIN master..spt_values b
)
INSERT INTO WorkerSkills (WorkerID, CategoryID, SkillLevel, YearsExperience)
SELECT
    w.WorkerID,
    ((ABS(CHECKSUM(NEWID())) % 8) + 1),
    CASE
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 66 THEN 'Expert'
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 33 THEN 'Intermediate'
        ELSE 'Beginner'
    END,
    (ABS(CHECKSUM(NEWID())) % 20) + 1
FROM NumberedRows n
JOIN WorkerIDs w ON w.RowNum = ((n.rn - 1) % 25000) + 1;

PRINT 'Worker Skills inserted (250,000 records).';
GO

-- Insert Jobs (400,000 records) - Reference actual Customer IDs
PRINT 'Inserting Jobs (400,000 records) - This will take a few minutes...';
WITH CustomerIDs AS (
    SELECT CustomerID, ROW_NUMBER() OVER (ORDER BY CustomerID) AS RowNum
    FROM Customers
),
NumberedRows AS (
    SELECT TOP 400000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM master..spt_values a CROSS JOIN master..spt_values b CROSS JOIN master..spt_values c
)
INSERT INTO Jobs (CustomerID, CategoryID, Title, Description, Budget, PostedDate, StartDate, EndDate,
                  Location, Status, UrgencyLevel, RequiredWorkers)
SELECT
    c.CustomerID,
    ((ABS(CHECKSUM(NEWID())) % 8) + 1),
    'Job Title ' + CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR),
    'Job Description for service request number ' + CAST(n.rn AS VARCHAR),
    CAST(100 + (ABS(CHECKSUM(NEWID())) % 5000) AS DECIMAL(10,2)),
    DATEADD(DAY, -(ABS(CHECKSUM(NEWID())) % 365), GETDATE()),
    DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 30), GETDATE()),
    DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 60) + 30, GETDATE()),
    'Location ' + CAST(ABS(CHECKSUM(NEWID())) % 5000 AS VARCHAR),
    CASE WHEN (ABS(CHECKSUM(NEWID())) % 100) > 50 THEN 'Open' ELSE 'Assigned' END,
    CASE
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 75 THEN 'Urgent'
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 50 THEN 'High'
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 25 THEN 'Medium'
        ELSE 'Low'
    END,
    (ABS(CHECKSUM(NEWID())) % 3) + 1
FROM NumberedRows n
JOIN CustomerIDs c ON c.RowNum = ((n.rn - 1) % 25000) + 1;

PRINT 'Jobs inserted (400,000 records).';
GO

-- Insert Bids (500,000 records) - Reference actual Job and Worker IDs
PRINT 'Inserting Bids (500,000 records) - This will take a few minutes...';
WITH JobIDs AS (
    SELECT JobID, ROW_NUMBER() OVER (ORDER BY JobID) AS RowNum
    FROM Jobs
),
WorkerIDs AS (
    SELECT WorkerID, ROW_NUMBER() OVER (ORDER BY WorkerID) AS RowNum
    FROM Workers
),
NumberedRows AS (
    SELECT TOP 500000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM master..spt_values a CROSS JOIN master..spt_values b CROSS JOIN master..spt_values c
)
INSERT INTO Bids (JobID, WorkerID, BidAmount, ProposedStartTime, EstimatedDuration, BidDate, Status, IsWinningBid)
SELECT
    j.JobID,
    w.WorkerID,
    CAST(50 + (ABS(CHECKSUM(NEWID())) % 2000) AS DECIMAL(10,2)),
    DATEADD(HOUR, (ABS(CHECKSUM(NEWID())) % 168), GETDATE()),
    (ABS(CHECKSUM(NEWID())) % 480) + 30,
    DATEADD(HOUR, -(ABS(CHECKSUM(NEWID())) % 168), GETDATE()),
    CASE
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 70 THEN 'Accepted'
        WHEN (ABS(CHECKSUM(NEWID())) % 100) > 30 THEN 'Pending'
        ELSE 'Rejected'
    END,
    CASE WHEN (ABS(CHECKSUM(NEWID())) % 100) > 95 THEN 1 ELSE 0 END
FROM NumberedRows n
JOIN JobIDs j ON j.RowNum = ((n.rn - 1) % 400000) + 1
JOIN WorkerIDs w ON w.RowNum = ((n.rn - 1) % 25000) + 1;

PRINT 'Bids inserted (500,000 records).';
GO

-- Insert Bookings
PRINT 'Inserting Bookings...';
INSERT INTO Bookings (JobID, WorkerID, BidID, ScheduledStart, ScheduledEnd, ActualStart, ActualEnd, Status, BookingCode)
SELECT TOP 150000
    b.JobID,
    b.WorkerID,
    b.BidID,
    DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 30), GETDATE()),
    DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 60) + 30, GETDATE()),
    DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 30) - 15, GETDATE()),
    DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % 60) + 15, GETDATE()),
    CASE WHEN (ABS(CHECKSUM(NEWID())) % 100) > 50 THEN 'Completed' ELSE 'InProgress' END,
    'BK' + CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR(20))
FROM Bids b
WHERE b.IsWinningBid = 1;

DECLARE @BookingsInserted INT = @@ROWCOUNT;
PRINT 'Bookings inserted (' + CAST(@BookingsInserted AS VARCHAR) + ' records).';
GO

PRINT 'Data population completed!';
GO

-- ========================================================================
-- SECTION 4: STORED PROCEDURES
-- ========================================================================

CREATE PROCEDURE sp_AcceptBid
    @BidID INT,
    @BookingCode VARCHAR(20) OUTPUT
AS
BEGIN
    BEGIN TRANSACTION;
    DECLARE @JobID INT, @WorkerID INT, @ProposedStartTime DATETIME;

    SELECT @JobID = JobID, @WorkerID = WorkerID, @ProposedStartTime = ProposedStartTime
    FROM Bids WHERE BidID = @BidID;

    UPDATE Bids SET Status = 'Accepted', IsWinningBid = 1 WHERE BidID = @BidID;
    UPDATE Bids SET Status = 'Rejected' WHERE JobID = @JobID AND BidID != @BidID AND Status = 'Pending';

    SET @BookingCode = 'BK' + CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR(20));

    INSERT INTO Bookings (JobID, WorkerID, BidID, ScheduledStart, BookingCode, Status)
    VALUES (@JobID, @WorkerID, @BidID, @ProposedStartTime, @BookingCode, 'Scheduled');

    UPDATE Jobs SET Status = 'Assigned' WHERE JobID = @JobID;
    COMMIT TRANSACTION;
END;
GO

CREATE PROCEDURE sp_CompleteBooking
    @BookingID INT,
    @CompletionNotes VARCHAR(MAX) = NULL
AS
BEGIN
    BEGIN TRANSACTION;
    UPDATE Bookings SET Status = 'Completed', ActualEnd = GETDATE(), CompletionNotes = @CompletionNotes
    WHERE BookingID = @BookingID;

    DECLARE @JobID INT;
    SELECT @JobID = JobID FROM Bookings WHERE BookingID = @BookingID;
    UPDATE Jobs SET Status = 'Completed' WHERE JobID = @JobID;
    COMMIT TRANSACTION;
END;
GO

CREATE PROCEDURE sp_GetAvailableWorkers
    @JobID INT,
    @CategoryID INT
AS
BEGIN
    SELECT w.WorkerID, w.FirstName, w.LastName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, ws.SkillLevel, ws.YearsExperience
    FROM Workers w
    INNER JOIN WorkerSkills ws ON w.WorkerID = ws.WorkerID
    WHERE ws.CategoryID = @CategoryID AND w.OverallRating >= 3.0 AND w.TotalJobsCompleted > 0
    ORDER BY w.OverallRating DESC, w.TotalJobsCompleted DESC;
END;
GO

CREATE PROCEDURE sp_GetWorkerPerformance
    @WorkerID INT
AS
BEGIN
    SELECT
        w.WorkerID, w.FirstName + ' ' + w.LastName AS FullName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted,
        COUNT(DISTINCT b.BidID) AS TotalBidsPlaced,
        COUNT(DISTINCT CASE WHEN b.IsWinningBid = 1 THEN b.BidID END) AS WinningBids,
        AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
        COUNT(DISTINCT r.ReviewID) AS TotalReviews
    FROM Workers w
    LEFT JOIN Bids b ON w.WorkerID = b.WorkerID
    LEFT JOIN Reviews r ON w.WorkerID = r.ReviewedID
    WHERE w.WorkerID = @WorkerID
    GROUP BY w.WorkerID, w.FirstName, w.LastName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted;
END;
GO

CREATE PROCEDURE sp_PlaceBid
    @JobID INT,
    @WorkerID INT,
    @BidAmount DECIMAL(10,2),
    @ProposedStartTime DATETIME,
    @EstimatedDuration INT,
    @CoverLetter VARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO Bids (JobID, WorkerID, BidAmount, ProposedStartTime, EstimatedDuration, CoverLetter, BidDate, Status)
    VALUES (@JobID, @WorkerID, @BidAmount, @ProposedStartTime, @EstimatedDuration, @CoverLetter, GETDATE(), 'Pending');
END;
GO

CREATE PROCEDURE sp_GetJobDetails
    @JobID INT
AS
BEGIN
    SELECT
        j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Budget, j.PostedDate,
        j.StartDate, j.EndDate, j.Location, j.Status, j.UrgencyLevel, j.RequiredWorkers, j.CompletedWorkers,
        c.FirstName + ' ' + c.LastName AS CustomerName,
        sc.CategoryName,
        COUNT(b.BidID) AS TotalBids,
        MIN(b.BidAmount) AS LowestBid,
        AVG(b.BidAmount) AS AverageBid
    FROM Jobs j
    INNER JOIN Customers c ON j.CustomerID = c.CustomerID
    INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
    LEFT JOIN Bids b ON j.JobID = b.JobID
    WHERE j.JobID = @JobID
    GROUP BY j.JobID, j.CustomerID, j.CategoryID, j.Title, j.Budget, j.PostedDate,
             j.StartDate, j.EndDate, j.Location, j.Status, j.UrgencyLevel,
             j.RequiredWorkers, j.CompletedWorkers, c.FirstName, c.LastName, sc.CategoryName;
END;
GO

PRINT 'Stored procedures created (6 procedures).';
GO

-- ========================================================================
-- SECTION 5: FUNCTIONS
-- ========================================================================

CREATE FUNCTION fn_CalculateJobComplexity(@BudgetAmount DECIMAL(10,2), @UrgencyLevel VARCHAR(20), @RequiredWorkers INT)
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 0;
    IF @BudgetAmount > 5000 SET @Score = @Score + 30;
    ELSE IF @BudgetAmount > 2000 SET @Score = @Score + 20;
    ELSE IF @BudgetAmount > 500 SET @Score = @Score + 10;

    IF @UrgencyLevel = 'Urgent' SET @Score = @Score + 40;
    ELSE IF @UrgencyLevel = 'High' SET @Score = @Score + 30;
    ELSE IF @UrgencyLevel = 'Medium' SET @Score = @Score + 15;

    SET @Score = @Score + (@RequiredWorkers * 10);
    RETURN @Score;
END;
GO

CREATE FUNCTION fn_GetWorkerReliabilityScore(@WorkerID INT)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @CompletedJobs INT = 0, @TotalBookings INT = 0, @ReliabilityScore DECIMAL(5,2) = 0;
    SELECT @TotalBookings = COUNT(*) FROM Bookings WHERE WorkerID = @WorkerID;
    SELECT @CompletedJobs = COUNT(*) FROM Bookings WHERE WorkerID = @WorkerID AND Status = 'Completed';

    IF @TotalBookings > 0
        SET @ReliabilityScore = (@CompletedJobs * 100.0) / @TotalBookings;

    RETURN @ReliabilityScore;
END;
GO

CREATE FUNCTION fn_GetJobsByLocation(@City VARCHAR(50), @CategoryID INT)
RETURNS TABLE
AS
RETURN (
    SELECT j.JobID, j.Title, j.Budget, j.Status, j.UrgencyLevel, j.PostedDate,
           c.FirstName + ' ' + c.LastName AS CustomerName, sc.CategoryName
    FROM Jobs j
    INNER JOIN Customers c ON j.CustomerID = c.CustomerID
    INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
    WHERE j.Location LIKE '%' + @City + '%' AND j.CategoryID = @CategoryID AND j.Status IN ('Open', 'Assigned')
);
GO

CREATE FUNCTION fn_GetBidStats(@JobID INT)
RETURNS TABLE
AS
RETURN (
    SELECT @JobID AS JobID, COUNT(*) AS TotalBids, AVG(BidAmount) AS AverageBidAmount,
           MIN(BidAmount) AS MinBidAmount, MAX(BidAmount) AS MaxBidAmount,
           COUNT(CASE WHEN Status = 'Accepted' THEN 1 END) AS AcceptedBids
    FROM Bids WHERE JobID = @JobID
);
GO

PRINT 'Functions created (4 functions).';
GO

-- ========================================================================
-- SECTION 6: TRIGGERS
-- ========================================================================

CREATE TRIGGER trg_UpdateWorkerRatingOnReview ON Reviews AFTER INSERT
AS
BEGIN
    UPDATE w SET w.OverallRating = (SELECT AVG(CAST(r.Rating AS FLOAT)) FROM Reviews r WHERE r.ReviewedID = w.WorkerID)
    FROM Workers w INNER JOIN inserted i ON w.WorkerID = i.ReviewedID;
END;
GO

CREATE TRIGGER trg_NotifyOnBidAccepted ON Bids AFTER UPDATE
AS
BEGIN
    INSERT INTO Notifications (UserID, NotificationType, Title, Message, RelatedEntityID, RelatedEntityType)
    SELECT WorkerID, 'BidAccepted', 'Your Bid Has Been Accepted!',
           'Congratulations! Your bid has been accepted for Job ID: ' + CAST(JobID AS VARCHAR), JobID, 'Job'
    FROM inserted WHERE Status = 'Accepted';
END;
GO

CREATE TRIGGER trg_UpdateJobCompletionOnBooking ON Bookings AFTER UPDATE
AS
BEGIN
    UPDATE j SET j.CompletedWorkers = (SELECT COUNT(*) FROM Bookings b WHERE b.JobID = j.JobID AND b.Status = 'Completed')
    FROM Jobs j INNER JOIN inserted i ON j.JobID = i.JobID;
END;
GO

CREATE TRIGGER trg_PreventDeleteCompletedBooking ON Bookings INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Status = 'Completed')
    BEGIN
        RAISERROR('Cannot delete completed bookings for audit purposes', 16, 1);
        ROLLBACK;
    END
    ELSE DELETE FROM Bookings WHERE BookingID IN (SELECT BookingID FROM deleted);
END;
GO

CREATE TRIGGER trg_ValidateBidAmount ON Bids INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Bids (JobID, WorkerID, BidAmount, ProposedStartTime, EstimatedDuration, CoverLetter, BidDate, Status, IsWinningBid)
    SELECT JobID, WorkerID, CASE WHEN BidAmount < 0 THEN ABS(BidAmount) ELSE BidAmount END, ProposedStartTime,
           CASE WHEN EstimatedDuration < 30 THEN 30 ELSE EstimatedDuration END, CoverLetter, BidDate, Status, IsWinningBid
    FROM inserted WHERE BidAmount > 0 AND EstimatedDuration > 0;
END;
GO

PRINT 'Triggers created (5 triggers).';
GO

-- ========================================================================
-- SECTION 7: VIEWS
-- ========================================================================

CREATE VIEW vw_ActiveJobsWithBids AS
SELECT j.JobID, j.Title, j.Budget, j.Status, j.UrgencyLevel, j.PostedDate, j.RequiredWorkers,
       c.FirstName + ' ' + c.LastName AS CustomerName, sc.CategoryName,
       COUNT(b.BidID) AS TotalBids,
       COUNT(CASE WHEN b.Status = 'Accepted' THEN 1 END) AS AcceptedBids
FROM Jobs j
INNER JOIN Customers c ON j.CustomerID = c.CustomerID
INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
LEFT JOIN Bids b ON j.JobID = b.JobID
WHERE j.Status IN ('Open', 'Assigned')
GROUP BY j.JobID, j.Title, j.Budget, j.Status, j.UrgencyLevel, j.PostedDate, j.RequiredWorkers, c.FirstName, c.LastName, sc.CategoryName;
GO

CREATE VIEW vw_TopRatedWorkers AS
SELECT w.WorkerID, w.FirstName + ' ' + w.LastName AS FullName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, w.City,
       sc.CategoryName, ws.SkillLevel, COUNT(DISTINCT r.ReviewID) AS ReviewCount, AVG(CAST(r.Rating AS FLOAT)) AS AverageRating
FROM Workers w
LEFT JOIN WorkerSkills ws ON w.WorkerID = ws.WorkerID
LEFT JOIN ServiceCategories sc ON ws.CategoryID = sc.CategoryID
LEFT JOIN Reviews r ON w.WorkerID = r.ReviewedID
GROUP BY w.WorkerID, w.FirstName, w.LastName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, w.City, sc.CategoryName, ws.SkillLevel;
GO

CREATE VIEW vw_BookingSummaryByCategory AS
SELECT sc.CategoryName,
       COUNT(CASE WHEN b.Status = 'Scheduled' THEN 1 END) AS ScheduledCount,
       COUNT(CASE WHEN b.Status = 'InProgress' THEN 1 END) AS InProgressCount,
       COUNT(CASE WHEN b.Status = 'Completed' THEN 1 END) AS CompletedCount,
       COUNT(CASE WHEN b.Status = 'Cancelled' THEN 1 END) AS CancelledCount,
       COUNT(*) AS TotalBookings, AVG(CAST(r.Rating AS FLOAT)) AS AverageCompletionRating
FROM Bookings b
INNER JOIN Jobs j ON b.JobID = j.JobID
INNER JOIN ServiceCategories sc ON j.CategoryID = sc.CategoryID
LEFT JOIN Reviews r ON b.BookingID = r.BookingID
GROUP BY sc.CategoryName;
GO

CREATE VIEW vw_CustomerAnalytics AS
SELECT c.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName, c.City, c.CustomerRating, c.TotalJobsPosted,
       COUNT(DISTINCT j.JobID) AS ActiveJobs, COUNT(DISTINCT b.BookingID) AS CompletedJobs, SUM(j.Budget) AS TotalSpend
FROM Customers c
LEFT JOIN Jobs j ON c.CustomerID = j.CustomerID AND j.Status IN ('Open', 'Assigned')
LEFT JOIN Bookings b ON j.JobID = b.JobID AND b.Status = 'Completed'
GROUP BY c.CustomerID, c.FirstName, c.LastName, c.City, c.CustomerRating, c.TotalJobsPosted;
GO

PRINT 'Views created (4 views).';
GO

-- ========================================================================
-- SECTION 8: CTEs
-- ========================================================================

CREATE PROCEDURE sp_TopPerformersByCategory @CategoryID INT
AS
BEGIN
    WITH WorkerPerformance AS (
        SELECT w.WorkerID, w.FirstName + ' ' + w.LastName AS FullName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, ws.CategoryID,
               COUNT(b.BidID) AS TotalBids, COUNT(CASE WHEN b.IsWinningBid = 1 THEN 1 END) AS WinningBids,
               CAST(COUNT(CASE WHEN b.IsWinningBid = 1 THEN 1 END) * 100.0 / NULLIF(COUNT(b.BidID), 0) AS DECIMAL(5,2)) AS WinRatePercentage,
               ROW_NUMBER() OVER (PARTITION BY ws.CategoryID ORDER BY w.OverallRating DESC, w.TotalJobsCompleted DESC) AS PerformanceRank
        FROM Workers w
        INNER JOIN WorkerSkills ws ON w.WorkerID = ws.WorkerID
        LEFT JOIN Bids b ON w.WorkerID = b.WorkerID
        WHERE ws.CategoryID = @CategoryID
        GROUP BY w.WorkerID, w.FirstName, w.LastName, w.HourlyRate, w.OverallRating, w.TotalJobsCompleted, ws.CategoryID
    )
    SELECT FullName, HourlyRate, OverallRating, TotalJobsCompleted, TotalBids, WinningBids, WinRatePercentage, PerformanceRank
    FROM WorkerPerformance WHERE PerformanceRank <= 20 ORDER BY PerformanceRank;
END;
GO

CREATE PROCEDURE sp_ComplexJobAnalysis
AS
BEGIN
    WITH JobMetrics AS (
        SELECT j.JobID, j.Title, j.Budget, j.Status, j.UrgencyLevel,
               COUNT(b.BidID) AS BidCount, AVG(b.BidAmount) AS AvgBidAmount, MIN(b.BidAmount) AS MinBidAmount, MAX(b.BidAmount) AS MaxBidAmount
        FROM Jobs j LEFT JOIN Bids b ON j.JobID = b.JobID
        GROUP BY j.JobID, j.Title, j.Budget, j.Status, j.UrgencyLevel
    ),
    JobComplexity AS (
        SELECT jm.*, dbo.fn_CalculateJobComplexity(jm.Budget, jm.UrgencyLevel, 1) AS ComplexityScore,
               ROW_NUMBER() OVER (ORDER BY jm.Budget DESC) AS BudgetRank
        FROM JobMetrics jm
    )
    SELECT TOP 100 JobID, Title, Budget, Status, UrgencyLevel, BidCount, AvgBidAmount, ComplexityScore, BudgetRank
    FROM JobComplexity WHERE BidCount > 0 ORDER BY ComplexityScore DESC;
END;
GO

PRINT 'CTE-based stored procedures created (2 procedures).';
GO

-- ========================================================================
-- SECTION 9: VERIFY DATA
-- ========================================================================

DECLARE @UsersCount INT, @WorkersCount INT, @CustomersCount INT, @SkillsCount INT;
DECLARE @JobsCount INT, @BidsCount INT, @BookingsCount INT, @TotalCount INT;

SELECT @UsersCount = COUNT(*) FROM Users;
SELECT @WorkersCount = COUNT(*) FROM Workers;
SELECT @CustomersCount = COUNT(*) FROM Customers;
SELECT @SkillsCount = COUNT(*) FROM WorkerSkills;
SELECT @JobsCount = COUNT(*) FROM Jobs;
SELECT @BidsCount = COUNT(*) FROM Bids;
SELECT @BookingsCount = COUNT(*) FROM Bookings;

SET @TotalCount = @UsersCount + @WorkersCount + @CustomersCount + @SkillsCount + @JobsCount + @BidsCount + @BookingsCount;

PRINT '';
PRINT '========================================================';
PRINT '=== DATA SUMMARY ===';
PRINT '========================================================';
PRINT 'Total Users: ' + CAST(@UsersCount AS VARCHAR);
PRINT 'Total Workers: ' + CAST(@WorkersCount AS VARCHAR);
PRINT 'Total Customers: ' + CAST(@CustomersCount AS VARCHAR);
PRINT 'Total Worker Skills: ' + CAST(@SkillsCount AS VARCHAR);
PRINT 'Total Jobs: ' + CAST(@JobsCount AS VARCHAR);
PRINT 'Total Bids: ' + CAST(@BidsCount AS VARCHAR);
PRINT 'Total Bookings: ' + CAST(@BookingsCount AS VARCHAR);
PRINT 'Grand Total Rows: ' + CAST(@TotalCount AS VARCHAR);
PRINT '';
PRINT '========================================================';
PRINT '=== DATABASE SETUP SUCCESSFULLY COMPLETED ===';
PRINT '========================================================';
PRINT '';
PRINT 'ServiceConnect database is now ready for use!';
PRINT '';
PRINT 'Features Implemented:';
PRINT '  - 13 Tables with complete referential integrity';
PRINT '  - 6 Stored Procedures (with CTEs)';
PRINT '  - 4 Functions (2 Scalar + 2 Table-Valued)';
PRINT '  - 5 Triggers (3 AFTER + 2 INSTEAD-OF)';
PRINT '  - 4 Views';
PRINT '  - 2 Complex CTEs (in stored procedures)';
PRINT '  - 16+ Strategic Indexes';
PRINT '';
PRINT 'All requirements for Phase 2 have been met!';
PRINT '========================================================';
GO
