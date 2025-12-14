using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.LinqEF;

public class JobServiceEF : IJobService
{
    private readonly ServiceConnectDbContext _context;

    public JobServiceEF(ServiceConnectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobDTO>> GetAllJobsAsync()
    {
        return await _context.Jobs
            .Include(j => j.Customer)
            .Include(j => j.Category)
            .Select(j => new JobDTO
            {
                JobID = j.JobID,
                CustomerID = j.CustomerID,
                CategoryID = j.CategoryID,
                Title = j.Title,
                Description = j.Description,
                Budget = j.Budget,
                PostedDate = j.PostedDate,
                StartDate = j.StartDate,
                EndDate = j.EndDate,
                Location = j.Location,
                Status = j.Status,
                UrgencyLevel = j.UrgencyLevel,
                RequiredWorkers = j.RequiredWorkers,
                CustomerName = j.Customer.FirstName + " " + j.Customer.LastName,
                CategoryName = j.Category.CategoryName
            })
            .OrderByDescending(j => j.PostedDate)
            .Take(100)
            .ToListAsync();
    }

    public async Task<JobDTO?> GetJobByIdAsync(int jobId)
    {
        var job = await _context.Jobs
            .Include(j => j.Customer)
            .Include(j => j.Category)
            .FirstOrDefaultAsync(j => j.JobID == jobId);

        if (job == null) return null;

        return new JobDTO
        {
            JobID = job.JobID,
            CustomerID = job.CustomerID,
            CategoryID = job.CategoryID,
            Title = job.Title,
            Description = job.Description,
            Budget = job.Budget,
            PostedDate = job.PostedDate,
            StartDate = job.StartDate,
            EndDate = job.EndDate,
            Location = job.Location,
            Status = job.Status,
            UrgencyLevel = job.UrgencyLevel,
            RequiredWorkers = job.RequiredWorkers,
            CustomerName = job.Customer.FirstName + " " + job.Customer.LastName,
            CategoryName = job.Category.CategoryName
        };
    }

    public async Task<JobDTO> CreateJobAsync(JobDTO jobDto)
    {
        var job = new Job
        {
            CustomerID = jobDto.CustomerID,
            CategoryID = jobDto.CategoryID,
            Title = jobDto.Title,
            Description = jobDto.Description,
            Budget = jobDto.Budget,
            PostedDate = DateTime.Now,
            StartDate = jobDto.StartDate,
            EndDate = jobDto.EndDate,
            Location = jobDto.Location,
            Status = "Open",
            UrgencyLevel = jobDto.UrgencyLevel,
            RequiredWorkers = jobDto.RequiredWorkers
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        jobDto.JobID = job.JobID;
        jobDto.PostedDate = job.PostedDate;
        jobDto.Status = job.Status;

        return jobDto;
    }

    public async Task<bool> UpdateJobAsync(JobDTO jobDto)
    {
        var job = await _context.Jobs.FindAsync(jobDto.JobID);
        if (job == null) return false;

        job.Title = jobDto.Title;
        job.Description = jobDto.Description;
        job.Budget = jobDto.Budget;
        job.StartDate = jobDto.StartDate;
        job.EndDate = jobDto.EndDate;
        job.Location = jobDto.Location;
        job.Status = jobDto.Status ?? job.Status;
        job.UrgencyLevel = jobDto.UrgencyLevel;
        job.RequiredWorkers = jobDto.RequiredWorkers;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null) return false;

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<JobDTO>> GetOpenJobsAsync()
    {
        // use linq query
        return await _context.Jobs
            .Where(j => j.Status == "Open")
            .Include(j => j.Customer)
            .Include(j => j.Category)
            .OrderByDescending(j => j.PostedDate)
            .Select(j => new JobDTO
            {
                JobID = j.JobID,
                CustomerID = j.CustomerID,
                CategoryID = j.CategoryID,
                Title = j.Title,
                Description = j.Description,
                Budget = j.Budget,
                PostedDate = j.PostedDate,
                Location = j.Location,
                Status = j.Status,
                UrgencyLevel = j.UrgencyLevel,
                RequiredWorkers = j.RequiredWorkers,
                CustomerName = j.Customer.FirstName + " " + j.Customer.LastName,
                CategoryName = j.Category.CategoryName
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<JobDTO>> GetJobsByCategoryAsync(int categoryId)
    {
        return await _context.Jobs
            .Where(j => j.CategoryID == categoryId)
            .Include(j => j.Customer)
            .Include(j => j.Category)
            .Select(j => new JobDTO
            {
                JobID = j.JobID,
                CustomerID = j.CustomerID,
                CategoryID = j.CategoryID,
                Title = j.Title,
                Description = j.Description,
                Budget = j.Budget,
                PostedDate = j.PostedDate,
                Location = j.Location,
                Status = j.Status,
                UrgencyLevel = j.UrgencyLevel,
                CustomerName = j.Customer.FirstName + " " + j.Customer.LastName,
                CategoryName = j.Category.CategoryName
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<JobDTO>> GetJobsByCustomerAsync(int customerId)
    {
        return await _context.Jobs
            .Where(j => j.CustomerID == customerId)
            .Include(j => j.Category)
            .OrderByDescending(j => j.PostedDate)
            .Select(j => new JobDTO
            {
                JobID = j.JobID,
                CustomerID = j.CustomerID,
                CategoryID = j.CategoryID,
                Title = j.Title,
                Description = j.Description,
                Budget = j.Budget,
                PostedDate = j.PostedDate,
                Location = j.Location,
                Status = j.Status,
                UrgencyLevel = j.UrgencyLevel,
                CategoryName = j.Category.CategoryName
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<JobDTO>> GetJobsByLocationAsync(string city, int categoryId)
    {
        // use table function via linq
        var query = from j in _context.Jobs
                    join c in _context.Customers on j.CustomerID equals c.CustomerID
                    join sc in _context.ServiceCategories on j.CategoryID equals sc.CategoryID
                    where j.Location!.Contains(city) && j.CategoryID == categoryId
                    where j.Status == "Open" || j.Status == "Assigned"
                    select new JobDTO
                    {
                        JobID = j.JobID,
                        Title = j.Title,
                        Budget = j.Budget,
                        Status = j.Status,
                        UrgencyLevel = j.UrgencyLevel,
                        PostedDate = j.PostedDate,
                        CustomerName = c.FirstName + " " + c.LastName,
                        CategoryName = sc.CategoryName
                    };

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<object>> GetActiveJobsWithBidsAsync()
    {
        // used view query vw_ActiveJobsWithBids
        var activeJobs = await (from j in _context.Jobs
                                join c in _context.Customers on j.CustomerID equals c.CustomerID
                                join sc in _context.ServiceCategories on j.CategoryID equals sc.CategoryID
                                where j.Status == "Open" || j.Status == "Assigned"
                                select new
                                {
                                    j.JobID,
                                    j.Title,
                                    j.Budget,
                                    j.Status,
                                    j.UrgencyLevel,
                                    j.PostedDate,
                                    j.RequiredWorkers,
                                    CustomerName = c.FirstName + " " + c.LastName,
                                    CategoryName = sc.CategoryName,
                                    TotalBids = _context.Bids.Count(b => b.JobID == j.JobID),
                                    AcceptedBids = _context.Bids.Count(b => b.JobID == j.JobID && b.Status == "Accepted")
                                }).ToListAsync();

        return activeJobs;
    }

    public async Task<int> CalculateJobComplexityAsync(decimal budget, string urgencyLevel, int requiredWorkers)
    {
        // showingscalar function fn_CalculateJobComplexity by linq
        int score = 0;

        if (budget > 5000) score += 30;
        else if (budget > 2000) score += 20;
        else if (budget > 500) score += 10;

        if (urgencyLevel == "Urgent") score += 40;
        else if (urgencyLevel == "High") score += 30;
        else if (urgencyLevel == "Medium") score += 15;

        score += requiredWorkers * 10;

        return await Task.FromResult(score);
    }
}
