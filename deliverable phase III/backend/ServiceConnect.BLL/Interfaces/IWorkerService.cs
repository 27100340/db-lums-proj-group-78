using ServiceConnect.Models.DTOs;

namespace ServiceConnect.BLL.Interfaces;

public interface IWorkerService
{
    // CRUD Operations
    Task<IEnumerable<WorkerDTO>> GetAllWorkersAsync();
    Task<WorkerDTO?> GetWorkerByIdAsync(int workerId);
    Task<WorkerDTO> CreateWorkerAsync(WorkerDTO workerDto);
    Task<bool> UpdateWorkerAsync(WorkerDTO workerDto);
    Task<bool> DeleteWorkerAsync(int workerId);

    // Business Logic Operations
    Task<IEnumerable<WorkerDTO>> GetWorkersBySkillAsync(int categoryId);
    Task<IEnumerable<WorkerDTO>> GetWorkersByCityAsync(string city);

    // Using Stored Procedures
    Task<IEnumerable<object>> GetAvailableWorkersForJobAsync(int jobId, int categoryId);
    Task<object> GetWorkerPerformanceAsync(int workerId);
    Task<IEnumerable<object>> GetTopPerformersByCategoryAsync(int categoryId);

    // Using Functions
    Task<decimal> GetWorkerReliabilityScoreAsync(int workerId);

    // Using Views
    Task<IEnumerable<object>> GetTopRatedWorkersAsync();
}
