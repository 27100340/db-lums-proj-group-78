using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.LinqEF;

public class WorkerServiceEF : IWorkerService
{
    private readonly ServiceConnectDbContext _context;

    public WorkerServiceEF(ServiceConnectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkerDTO>> GetAllWorkersAsync()
    {
        return await _context.Workers
            .Include(w => w.User)
            .Select(w => new WorkerDTO
            {
                WorkerID = w.WorkerID,
                Email = w.User.Email,
                FirstName = w.FirstName,
                LastName = w.LastName,
                PhoneNumber = w.User.PhoneNumber,
                DateOfBirth = w.DateOfBirth,
                Address = w.Address,
                City = w.City,
                PostalCode = w.PostalCode,
                HourlyRate = w.HourlyRate,
                OverallRating = w.OverallRating,
                TotalJobsCompleted = w.TotalJobsCompleted,
                Bio = w.Bio
            })
            .ToListAsync();
    }

    public async Task<WorkerDTO?> GetWorkerByIdAsync(int workerId)
    {
        var worker = await _context.Workers
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.WorkerID == workerId);

        if (worker == null) return null;

        return new WorkerDTO
        {
            WorkerID = worker.WorkerID,
            Email = worker.User.Email,
            FirstName = worker.FirstName,
            LastName = worker.LastName,
            PhoneNumber = worker.User.PhoneNumber,
            DateOfBirth = worker.DateOfBirth,
            Address = worker.Address,
            City = worker.City,
            PostalCode = worker.PostalCode,
            HourlyRate = worker.HourlyRate,
            OverallRating = worker.OverallRating,
            TotalJobsCompleted = worker.TotalJobsCompleted,
            Bio = worker.Bio
        };
    }

    public async Task<WorkerDTO> CreateWorkerAsync(WorkerDTO workerDto)
    {
        // make user
        var user = new User
        {
            Email = workerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(workerDto.Password ?? "defaultPassword123"),
            PhoneNumber = workerDto.PhoneNumber,
            UserType = "Worker",
            RegistrationDate = DateTime.Now,
            IsVerified = false,
            AccountStatus = "Active"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // make worker
        var worker = new Worker
        {
            WorkerID = user.UserID,
            FirstName = workerDto.FirstName,
            LastName = workerDto.LastName,
            DateOfBirth = workerDto.DateOfBirth,
            Address = workerDto.Address,
            City = workerDto.City,
            PostalCode = workerDto.PostalCode,
            HourlyRate = workerDto.HourlyRate,
            OverallRating = 0,
            TotalJobsCompleted = 0,
            Bio = workerDto.Bio
        };

        _context.Workers.Add(worker);
        await _context.SaveChangesAsync();

        workerDto.WorkerID = worker.WorkerID;
        return workerDto;
    }

    public async Task<bool> UpdateWorkerAsync(WorkerDTO workerDto)
    {
        var worker = await _context.Workers
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.WorkerID == workerDto.WorkerID);

        if (worker == null) return false;

        worker.FirstName = workerDto.FirstName;
        worker.LastName = workerDto.LastName;
        worker.DateOfBirth = workerDto.DateOfBirth;
        worker.Address = workerDto.Address;
        worker.City = workerDto.City;
        worker.PostalCode = workerDto.PostalCode;
        worker.HourlyRate = workerDto.HourlyRate;
        worker.Bio = workerDto.Bio;
        worker.User.PhoneNumber = workerDto.PhoneNumber;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteWorkerAsync(int workerId)
    {
        var worker = await _context.Workers.FindAsync(workerId);
        if (worker == null) return false;

        _context.Workers.Remove(worker);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<WorkerDTO>> GetWorkersBySkillAsync(int categoryId)
    {
        return await _context.Workers
            .Include(w => w.User)
            .Where(w => w.WorkerSkills.Any(ws => ws.CategoryID == categoryId))
            .Select(w => new WorkerDTO
            {
                WorkerID = w.WorkerID,
                Email = w.User.Email,
                FirstName = w.FirstName,
                LastName = w.LastName,
                City = w.City,
                HourlyRate = w.HourlyRate,
                OverallRating = w.OverallRating,
                TotalJobsCompleted = w.TotalJobsCompleted
            })
            .OrderByDescending(w => w.TotalJobsCompleted)
            .Take(100)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerDTO>> GetWorkersByCityAsync(string city)
    {
        return await _context.Workers
            .Include(w => w.User)
            .Where(w => w.City == city)
            .Select(w => new WorkerDTO
            {
                WorkerID = w.WorkerID,
                Email = w.User.Email,
                FirstName = w.FirstName,
                LastName = w.LastName,
                City = w.City,
                HourlyRate = w.HourlyRate,
                OverallRating = w.OverallRating,
                TotalJobsCompleted = w.TotalJobsCompleted
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetAvailableWorkersForJobAsync(int jobId, int categoryId)
    {
        // show sp_GetAvailableWorkers sproc with linq
        var availableWorkers = await (from w in _context.Workers
                                      join ws in _context.WorkerSkills on w.WorkerID equals ws.WorkerID
                                      where ws.CategoryID == categoryId
                                      where w.OverallRating >= 3.0m
                                      where w.TotalJobsCompleted > 0
                                      orderby w.OverallRating descending, w.TotalJobsCompleted descending
                                      select new
                                      {
                                          w.WorkerID,
                                          FullName = w.FirstName + " " + w.LastName,
                                          w.HourlyRate,
                                          w.OverallRating,
                                          w.TotalJobsCompleted,
                                          ws.SkillLevel,
                                          ws.YearsExperience
                                      }).ToListAsync();

        return availableWorkers;
    }

    public async Task<object> GetWorkerPerformanceAsync(int workerId)
    {
        // sp_GetWorkerPerformance sproc
        var worker = await _context.Workers.FindAsync(workerId);
        if (worker == null) return new { };

        var totalBids = await _context.Bids.CountAsync(b => b.WorkerID == workerId);
        var winningBids = await _context.Bids.CountAsync(b => b.WorkerID == workerId && b.IsWinningBid);
        var avgRating = await _context.Reviews
            .Where(r => r.ReviewedID == workerId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;
        var totalReviews = await _context.Reviews.CountAsync(r => r.ReviewedID == workerId);

        return new
        {
            worker.WorkerID,
            FullName = worker.FirstName + " " + worker.LastName,
            worker.HourlyRate,
            worker.OverallRating,
            worker.TotalJobsCompleted,
            TotalBidsPlaced = totalBids,
            WinningBids = winningBids,
            AverageRating = avgRating,
            TotalReviews = totalReviews
        };
    }

    public async Task<IEnumerable<object>> GetTopPerformersByCategoryAsync(int categoryId)
    {
        //  sp_TopPerformersByCategory sproc
        var topPerformers = await (from w in _context.Workers
                                   join ws in _context.WorkerSkills on w.WorkerID equals ws.WorkerID
                                   where ws.CategoryID == categoryId
                                   let bids = _context.Bids.Where(b => b.WorkerID == w.WorkerID)
                                   let winningBids = bids.Where(b => b.IsWinningBid)
                                   select new
                                   {
                                       w.WorkerID,
                                       FullName = w.FirstName + " " + w.LastName,
                                       w.HourlyRate,
                                       w.OverallRating,
                                       w.TotalJobsCompleted,
                                       TotalBids = bids.Count(),
                                       WinningBids = winningBids.Count(),
                                       WinRatePercentage = bids.Count() > 0 ? (winningBids.Count() * 100.0 / bids.Count()) : 0
                                   })
                                   .OrderByDescending(w => w.OverallRating)
                                   .ThenByDescending(w => w.TotalJobsCompleted)
                                   .Take(20)
                                   .ToListAsync();

        return topPerformers;
    }

    public async Task<decimal> GetWorkerReliabilityScoreAsync(int workerId)
    {
        // fn_GetWorkerReliabilityScore function
        var totalBookings = await _context.Bookings.CountAsync(b => b.WorkerID == workerId);
        var completedBookings = await _context.Bookings.CountAsync(b => b.WorkerID == workerId && b.Status == "Completed");

        if (totalBookings == 0) return 0;

        return (decimal)(completedBookings * 100.0 / totalBookings);
    }

    public async Task<IEnumerable<object>> GetTopRatedWorkersAsync()
    {
        // sing view query vw_TopRatedWorkers
        var topRated = await (from w in _context.Workers
                              join ws in _context.WorkerSkills on w.WorkerID equals ws.WorkerID into workerSkills
                              from ws in workerSkills.DefaultIfEmpty()
                              join sc in _context.ServiceCategories on ws.CategoryID equals sc.CategoryID into categories
                              from sc in categories.DefaultIfEmpty()
                              let reviews = _context.Reviews.Where(r => r.ReviewedID == w.WorkerID)
                              select new
                              {
                                  w.WorkerID,
                                  FullName = w.FirstName + " " + w.LastName,
                                  w.HourlyRate,
                                  w.OverallRating,
                                  w.TotalJobsCompleted,
                                  w.City,
                                  CategoryName = sc != null ? sc.CategoryName : null,
                                  SkillLevel = ws != null ? ws.SkillLevel : null,
                                  ReviewCount = reviews.Count(),
                                  AverageRating = reviews.Any() ? reviews.Average(r => (double?)r.Rating) : null
                              })
                              .OrderByDescending(w => w.OverallRating)
                              .ToListAsync();

        return topRated;
    }
}
