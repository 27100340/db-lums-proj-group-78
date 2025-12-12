# SQL Server Setup Instructions for Phase 3

## Prerequisites

* SQL Server 2019 or later installed and running
* SQL Server Management Studio (SSMS) installed
* SQL Server Authentication or Windows Authentication properly configured

## Step-by-Step Database Setup

### Step 1: Drop Existing Database

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance (commonly `localhost` or `.\\SQLEXPRESS`)
3. Open the file: `SQL_SCRIPTS\\RECREATE_DATABASE.sql`
4. Click **Execute** (or press **F5**)
5. Confirm that the output message shows: `ServiceConnect database dropped successfully!`

### Step 2: Create a Fresh Database with Full Dataset

1. In SSMS, navigate to the project root directory
2. Open the file: `ServiceConnect_Phase2.sql` (located in the main project root, not inside the SQL_SCRIPTS folder)
3. Click **Execute** (or press **F5**)
4. Allow the script to run for **5–10 minutes**, as it inserts over 1.3 million rows
5. Monitor the **Messages** tab for progress and completion indicators

### Step 3: Verify Database Creation

1. In SSMS, open the file: `SQL_SCRIPTS\\VERIFY_DATABASE.sql`
2. Click **Execute** (or press **F5**)
3. Review the **Results** tab to ensure all row counts and checks match expectations

#### Expected Row Counts

* **Users**: 50,000
* **Workers**: 25,000
* **Customers**: 25,000
* **ServiceCategories**: 8
* **WorkerSkills**: 250,000
* **Jobs**: 400,000
* **Bids**: 500,000
* **Bookings**: 150,000+

#### Expected SQL Server Features

* **Stored Procedures (6)**: sp_AcceptBid, sp_CompleteBooking, sp_GetAvailableWorkers, sp_GetWorkerPerformance, sp_TopPerformersByCategory, sp_ComplexJobAnalysis
* **Functions (4)**: fn_CalculateJobComplexity, fn_GetWorkerReliabilityScore, fn_GetJobsByLocation, fn_GetBidStats
* **Triggers (5)**: trg_UpdateWorkerRatingOnReview, trg_NotifyOnBidAccepted, trg_UpdateJobCompletionOnBooking, trg_PreventDeleteCompletedBooking, trg_ValidateBidAmount
* **Views (4)**: vw_ActiveJobsWithBids, vw_TopRatedWorkers, vw_BookingSummaryByCategory, vw_CustomerAnalytics
* **Indexes**: 15 or more non-clustered and covering indexes

### Step 4: Retrieve Connection String Information

Run the following queries in SSMS to identify your server and instance details:

```sql
SELECT @@SERVERNAME AS ServerName;
SELECT SERVERPROPERTY('InstanceName') AS InstanceName;
```

### Step 5: Configure Application Connection Settings

Using the information obtained in Step 4, update the `.env` file located in the `backend/ServiceConnect.API` directory.

#### Using Windows Authentication (recommended)

```
DB_SERVER=your_server_name
DB_NAME=ServiceConnect
DB_INTEGRATED_SECURITY=true
```

#### Using SQL Server Authentication

```
DB_SERVER=your_server_name
DB_NAME=ServiceConnect
DB_USER=your_username
DB_PASSWORD=your_password
DB_INTEGRATED_SECURITY=false
```

#### Common Server Name Examples

* `localhost` – Default SQL Server instance
* `.\\SQLEXPRESS` – SQL Server Express
* `.` – Local default instance
* `(localdb)\\MSSQLLocalDB` – SQL Server LocalDB

## Troubleshooting

### Error: Cannot open database

* Verify that the SQL Server service is running
* Check Windows Services and confirm that **SQL Server (MSSQLSERVER)** is in the *Running* state

### Error: Login failed

* For Windows Authentication: Ensure your Windows user account has access permissions in SQL Server
* For SQL Server Authentication: Double-check the username and password
* In SQL Server Configuration Manager, confirm that **TCP/IP** is enabled under *SQL Server Network Configuration*

### Error: Timeout expired

* The Phase 2 script inserts more than 1.3 million records and may take several minutes to complete
* Increase the query execution timeout in SSMS:

  * Tools → Options → Query Execution → SQL Server → General
  * Set *Execution time-out* to **0** (unlimited)

### Error: Objects already exist

* Re-run `RECREATE_DATABASE.sql` to remove existing objects
* Then execute `ServiceConnect_Phase2.sql` again from the beginning

## Next Steps

After the database has been successfully created and verified:

1. Update the backend `.env` file with the correct connection details
2. Install backend dependencies: `cd backend/ServiceConnect.API && dotnet restore`
3. Run the backend service: `dotnet run`
4. Install frontend dependencies: `cd frontend/serviceconnect-ui && npm install`
5. Start the frontend application: `npm run dev`
6. Access the application at: [http://localhost:3000](http://localhost:3000)
