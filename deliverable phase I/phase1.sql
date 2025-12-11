-- Create Database
CREATE DATABASE ServiceConnect;
GO
USE ServiceConnect;
GO

-- Create Tables with all relationships

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
    Bio TEXT,
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
    CategoryDescription TEXT,
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
    Description TEXT,
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
    CoverLetter TEXT,
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
    CompletionNotes TEXT,
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
    Comment TEXT,
    ReviewDate DATETIME DEFAULT GETDATE(),
    IsDisputed BIT DEFAULT 0,
    DisputeResolution TEXT,
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
    Message TEXT,
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsRead BIT DEFAULT 0,
    RelatedEntityID INT,
    RelatedEntityType VARCHAR(50),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Create Indexes for Performance
CREATE NONCLUSTERED INDEX IX_Jobs_CustomerID ON Jobs(CustomerID);
CREATE NONCLUSTERED INDEX IX_Jobs_CategoryID ON Jobs(CategoryID);
CREATE NONCLUSTERED INDEX IX_Jobs_Status ON Jobs(Status);
CREATE NONCLUSTERED INDEX IX_Bids_JobID ON Bids(JobID);
CREATE NONCLUSTERED INDEX IX_Bids_WorkerID ON Bids(WorkerID);
CREATE NONCLUSTERED INDEX IX_Bookings_JobID ON Bookings(JobID);
CREATE NONCLUSTERED INDEX IX_Bookings_WorkerID ON Bookings(WorkerID);
CREATE NONCLUSTERED INDEX IX_Reviews_BookingID ON Reviews(BookingID);
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);
CREATE NONCLUSTERED INDEX IX_Jobs_PostedDate ON Jobs(PostedDate);

-- Sample Stored Procedure for Complex Operation
CREATE PROCEDURE sp_AcceptBid
    @BidID INT,
    @BookingCode VARCHAR(20)
AS
BEGIN
    BEGIN TRANSACTION;
    
    DECLARE @JobID INT, @WorkerID INT;
    SELECT @JobID = JobID, @WorkerID = WorkerID FROM Bids WHERE BidID = @BidID;
    
    -- Update bid status
    UPDATE Bids SET Status = 'Accepted', IsWinningBid = 1 WHERE BidID = @BidID;
    UPDATE Bids SET Status = 'Rejected' WHERE JobID = @JobID AND BidID != @BidID;
    
    -- Create booking
    INSERT INTO Bookings (JobID, WorkerID, BidID, ScheduledStart, BookingCode, Status)
    SELECT JobID, WorkerID, BidID, ProposedStartTime, @BookingCode, 'Scheduled'
    FROM Bids WHERE BidID = @BidID;
    
    -- Update job status
    UPDATE Jobs SET Status = 'Assigned' WHERE JobID = @JobID;
    
    COMMIT TRANSACTION;
END;
GO