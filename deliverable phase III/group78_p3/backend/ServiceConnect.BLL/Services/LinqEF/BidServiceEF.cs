using Microsoft.EntityFrameworkCore;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.Models.DTOs;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Services.LinqEF;

public class BidServiceEF : IBidService
{
    private readonly ServiceConnectDbContext _context;

    public BidServiceEF(ServiceConnectDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BidDTO>> GetAllBidsAsync()
    {
        return await _context.Bids
            .Include(b => b.Worker)
            .Include(b => b.Job)
            .Select(b => new BidDTO
            {
                BidID = b.BidID,
                JobID = b.JobID,
                WorkerID = b.WorkerID,
                BidAmount = b.BidAmount ?? 0,
                ProposedStartTime = b.ProposedStartTime,
                EstimatedDuration = b.EstimatedDuration,
                CoverLetter = b.CoverLetter,
                BidDate = b.BidDate,
                Status = b.Status,
                IsWinningBid = b.IsWinningBid,
                WorkerName = b.Worker.FirstName + " " + b.Worker.LastName,
                JobTitle = b.Job.Title
            })
            .OrderByDescending(b => b.BidDate)
            .Take(100)
            .ToListAsync();
    }

    public async Task<BidDTO?> GetBidByIdAsync(int bidId)
    {
        var bid = await _context.Bids
            .Include(b => b.Worker)
            .Include(b => b.Job)
            .FirstOrDefaultAsync(b => b.BidID == bidId);

        if (bid == null) return null;

        return new BidDTO
        {
            BidID = bid.BidID,
            JobID = bid.JobID,
            WorkerID = bid.WorkerID,
            BidAmount = bid.BidAmount ?? 0,
            ProposedStartTime = bid.ProposedStartTime,
            EstimatedDuration = bid.EstimatedDuration,
            CoverLetter = bid.CoverLetter,
            BidDate = bid.BidDate,
            Status = bid.Status,
            IsWinningBid = bid.IsWinningBid,
            WorkerName = bid.Worker.FirstName + " " + bid.Worker.LastName,
            JobTitle = bid.Job.Title
        };
    }

    public async Task<BidDTO> CreateBidAsync(BidDTO bidDto)
    {
        var bid = new Bid
        {
            JobID = bidDto.JobID,
            WorkerID = bidDto.WorkerID,
            BidAmount = bidDto.BidAmount,
            ProposedStartTime = bidDto.ProposedStartTime,
            EstimatedDuration = bidDto.EstimatedDuration,
            CoverLetter = bidDto.CoverLetter,
            BidDate = DateTime.Now,
            Status = "Pending",
            IsWinningBid = false
        };

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        bidDto.BidID = bid.BidID;
        bidDto.BidDate = bid.BidDate;
        bidDto.Status = bid.Status;

        return bidDto;
    }

    public async Task<bool> UpdateBidAsync(BidDTO bidDto)
    {
        var bid = await _context.Bids.FindAsync(bidDto.BidID);
        if (bid == null) return false;

        bid.BidAmount = bidDto.BidAmount;
        bid.ProposedStartTime = bidDto.ProposedStartTime;
        bid.EstimatedDuration = bidDto.EstimatedDuration;
        bid.CoverLetter = bidDto.CoverLetter;
        bid.Status = bidDto.Status ?? bid.Status;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBidAsync(int bidId)
    {
        var bid = await _context.Bids.FindAsync(bidId);
        if (bid == null) return false;

        _context.Bids.Remove(bid);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BidDTO>> GetBidsByJobAsync(int jobId)
    {
        return await _context.Bids
            .Include(b => b.Worker)
            .Where(b => b.JobID == jobId)
            .Select(b => new BidDTO
            {
                BidID = b.BidID,
                JobID = b.JobID,
                WorkerID = b.WorkerID,
                BidAmount = b.BidAmount ?? 0,
                ProposedStartTime = b.ProposedStartTime,
                EstimatedDuration = b.EstimatedDuration,
                BidDate = b.BidDate,
                Status = b.Status,
                IsWinningBid = b.IsWinningBid,
                WorkerName = b.Worker.FirstName + " " + b.Worker.LastName
            })
            .OrderByDescending(b => b.BidDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BidDTO>> GetBidsByWorkerAsync(int workerId)
    {
        return await _context.Bids
            .Include(b => b.Job)
            .Where(b => b.WorkerID == workerId)
            .Select(b => new BidDTO
            {
                BidID = b.BidID,
                JobID = b.JobID,
                WorkerID = b.WorkerID,
                BidAmount = b.BidAmount ?? 0,
                ProposedStartTime = b.ProposedStartTime,
                EstimatedDuration = b.EstimatedDuration,
                BidDate = b.BidDate,
                Status = b.Status,
                IsWinningBid = b.IsWinningBid,
                JobTitle = b.Job.Title
            })
            .OrderByDescending(b => b.BidDate)
            .ToListAsync();
    }

    public async Task<string> AcceptBidAsync(int bidId)
    {
        // sp_AcceptBid sproc with linq
        // should trigger trg_NotifyOnBidAccepted 
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var bid = await _context.Bids
                .Include(b => b.Job)
                .FirstOrDefaultAsync(b => b.BidID == bidId);

            if (bid == null) throw new Exception("Bid not found");

            // bid status
            bid.Status = "Accepted";
            bid.IsWinningBid = true;

            // stopp other pending bids for same job
            var otherBids = await _context.Bids
                .Where(b => b.JobID == bid.JobID && b.BidID != bidId && b.Status == "Pending")
                .ToListAsync();

            foreach (var otherBid in otherBids)
            {
                otherBid.Status = "Rejected";
            }

            //  booking code creation
            var bookingCode = "BK" + new Random().Next(100000, 999999).ToString();

            // booking creation
            var booking = new Booking
            {
                JobID = bid.JobID,
                WorkerID = bid.WorkerID,
                BidID = bid.BidID,
                ScheduledStart = bid.ProposedStartTime,
                BookingCode = bookingCode,
                Status = "Scheduled"
            };

            _context.Bookings.Add(booking);

            // modify job status
            bid.Job.Status = "Assigned";

            await _context.SaveChangesAsync();

            // notification trigger
            var notification = new Notification
            {
                UserID = bid.WorkerID,
                NotificationType = "BidAccepted",
                Title = "Your Bid Has Been Accepted!",
                Message = $"Congratulations! Your bid has been accepted for Job ID: {bid.JobID}",
                CreatedDate = DateTime.Now,
                IsRead = false,
                RelatedEntityID = bid.JobID,
                RelatedEntityType = "Job"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return bookingCode;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<object> GetBidStatsAsync(int jobId)
    {
        //  fn_GetBidStats table-valued function
        var bids = await _context.Bids
            .Where(b => b.JobID == jobId)
            .ToListAsync();

        return new
        {
            JobID = jobId,
            TotalBids = bids.Count,
            AverageBidAmount = bids.Any() ? bids.Average(b => b.BidAmount) : 0,
            MinBidAmount = bids.Any() ? bids.Min(b => b.BidAmount) : 0,
            MaxBidAmount = bids.Any() ? bids.Max(b => b.BidAmount) : 0,
            AcceptedBids = bids.Count(b => b.Status == "Accepted")
        };
    }
}
