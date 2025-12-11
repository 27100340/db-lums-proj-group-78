# SQL Server Setup Instructions for Phase 3

## Prerequisites
- SQL Server 2019 or later installed and running
- SQL Server Management Studio (SSMS) installed
- SQL Server Authentication or Windows Authentication configured

## Step-by-Step Database Setup

### Step 1: Drop Existing Database
1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance (usually `localhost` or `.\SQLEXPRESS`)
3. Open the file: `SQL_SCRIPTS\RECREATE_DATABASE.sql`
4. Click **Execute** (or press F5)
5. Verify output shows: "ServiceConnect database dropped successfully!"

### Step 2: Create Fresh Database with All Data
1. In SSMS, navigate to the project root folder
2. Open the file: `ServiceConnect_Phase2.sql` (from main project root, not this folder)
3. Click **Execute** (or press F5)
4. **Wait 5-10 minutes** for script to complete (it's inserting 1.3M+ rows)
5. Watch the Messages tab for progress indicators

### Step 3: Verify Database Creation
1. In SSMS, open the file: `SQL_SCRIPTS\VERIFY_DATABASE.sql`
2. Click **Execute** (or press F5)
3. Check the Results tab to verify all row counts match expectations

Expected Results:
- **Users:** 50,000
- **Workers:** 25,000
- **Customers:** 25,000
- **ServiceCategories:** 8
- **WorkerSkills:** 250,000
- **Jobs:** 400,000
- **Bids:** 500,000
- **Bookings:** 150,000+

Expected SQL Features:
- **Stored Procedures:** 6 (sp_AcceptBid, sp_CompleteBooking, sp_GetAvailableWorkers, sp_GetWorkerPerformance, sp_TopPerformersByCategory, sp_ComplexJobAnalysis)
- **Functions:** 4 (fn_CalculateJobComplexity, fn_GetWorkerReliabilityScore, fn_GetJobsByLocation, fn_GetBidStats)
- **Triggers:** 5 (trg_UpdateWorkerRatingOnReview, trg_NotifyOnBidAccepted, trg_UpdateJobCompletionOnBooking, trg_PreventDeleteCompletedBooking, trg_ValidateBidAmount)
- **Views:** 4 (vw_ActiveJobsWithBids, vw_TopRatedWorkers, vw_BookingSummaryByCategory, vw_CustomerAnalytics)
- **Indexes:** 15+

### Step 4: Get Connection String Details
Run this query in SSMS:

```sql
SELECT @@SERVERNAME AS ServerName;
SELECT SERVERPROPERTY('InstanceName') AS InstanceName;
```

### Step 5: Configure Application Connection
Based on the results from Step 4, update your `.env` file in the `backend/ServiceConnect.API` folder.

**If using Windows Authentication (recommended):**
```
DB_SERVER=your_server_name
DB_NAME=ServiceConnect
DB_INTEGRATED_SECURITY=true
```

**If using SQL Server Authentication:**
```
DB_SERVER=your_server_name
DB_NAME=ServiceConnect
DB_USER=your_username
DB_PASSWORD=your_password
DB_INTEGRATED_SECURITY=false
```

Common server names:
- `localhost` - Default SQL Server instance
- `.\\SQLEXPRESS` - SQL Server Express
- `.` - Local default instance
- `(localdb)\\MSSQLLocalDB` - LocalDB

## Troubleshooting

### Error: "Cannot open database"
- Ensure SQL Server service is running
- Check Windows Services: SQL Server (MSSQLSERVER) should be "Running"

### Error: "Login failed"
- If using Windows Auth: Ensure your Windows user has SQL Server access
- If using SQL Auth: Verify username/password are correct
- Check SQL Server Configuration Manager > SQL Server Network Configuration > Protocols: TCP/IP should be Enabled

### Error: "Timeout expired"
- The Phase 2 script inserts 1.3M rows and may take 5-10 minutes
- Increase query timeout in SSMS: Tools > Options > Query Execution > SQL Server > General > Execution time-out (set to 0 for unlimited)

### Error: "Objects already exist"
- Run RECREATE_DATABASE.sql again to drop everything
- Then re-run ServiceConnect_Phase2.sql

## Next Steps
After successful database setup:
1. Configure backend `.env` file with connection details
2. Install backend dependencies: `cd backend/ServiceConnect.API && dotnet restore`
3. Run backend: `dotnet run`
4. Install frontend dependencies: `cd frontend/serviceconnect-ui && npm install`
5. Run frontend: `npm run dev`
6. Access application at http://localhost:3000
