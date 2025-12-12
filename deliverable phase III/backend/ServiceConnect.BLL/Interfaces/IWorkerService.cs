using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface IWorkerService
{
    // create, read, update, delete
    Task<IEnumerable<WorkerDTO>> GetAllWorkersAsync();
    Task<WorkerDTO?> GetWorkerByIdAsync(int workerId);
    Task<WorkerDTO> CreateWorkerAsync(WorkerDTO workerDto);
    Task<bool> UpdateWorkerAsync(WorkerDTO workerDto);
    Task<bool> DeleteWorkerAsync(int workerId);

    // logic operations
    Task<IEnumerable<WorkerDTO>> GetWorkersBySkillAsync(int categoryId);
    Task<IEnumerable<WorkerDTO>> GetWorkersByCityAsync(string city);

    // sprocs
    Task<IEnumerable<object>> GetAvailableWorkersForJobAsync(int jobId, int categoryId);
    Task<object> GetWorkerPerformanceAsync(int workerId);
    Task<IEnumerable<object>> GetTopPerformersByCategoryAsync(int categoryId);

    // functions
    Task<decimal> GetWorkerReliabilityScoreAsync(int workerId);

    // views
    Task<IEnumerable<object>> GetTopRatedWorkersAsync();
}
