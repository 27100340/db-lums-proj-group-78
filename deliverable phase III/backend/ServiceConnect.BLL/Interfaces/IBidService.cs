using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface IBidService
{
    // CRUD Operations
    Task<IEnumerable<BidDTO>> GetAllBidsAsync();
    Task<BidDTO?> GetBidByIdAsync(int bidId);
    Task<BidDTO> CreateBidAsync(BidDTO bidDto);
    Task<bool> UpdateBidAsync(BidDTO bidDto);
    Task<bool> DeleteBidAsync(int bidId);

    // Business Logic Operations
    Task<IEnumerable<BidDTO>> GetBidsByJobAsync(int jobId);
    Task<IEnumerable<BidDTO>> GetBidsByWorkerAsync(int workerId);

    // Using Stored Procedures (triggers notifications)
    Task<string> AcceptBidAsync(int bidId);

    // Using Functions
    Task<object> GetBidStatsAsync(int jobId);
}
