using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface IBidService
{
    // create,read,update,delete
    Task<IEnumerable<BidDTO>> GetAllBidsAsync();
    Task<BidDTO?> GetBidByIdAsync(int bidId);
    Task<BidDTO> CreateBidAsync(BidDTO bidDto);
    Task<bool> UpdateBidAsync(BidDTO bidDto);
    Task<bool> DeleteBidAsync(int bidId);

    // logic operations
    Task<IEnumerable<BidDTO>> GetBidsByJobAsync(int jobId);
    Task<IEnumerable<BidDTO>> GetBidsByWorkerAsync(int workerId);

    // sprocs
    Task<string> AcceptBidAsync(int bidId);

    // functions used
    Task<object> GetBidStatsAsync(int jobId);
}
