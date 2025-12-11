using ServiceConnect.Models.DTOs;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Interfaces;

public interface IJobService
{
    // CRUD Operations
    Task<IEnumerable<JobDTO>> GetAllJobsAsync();
    Task<JobDTO?> GetJobByIdAsync(int jobId);
    Task<JobDTO> CreateJobAsync(JobDTO jobDto);
    Task<bool> UpdateJobAsync(JobDTO jobDto);
    Task<bool> DeleteJobAsync(int jobId);

    // Business Logic Operations
    Task<IEnumerable<JobDTO>> GetOpenJobsAsync();
    Task<IEnumerable<JobDTO>> GetJobsByCategoryAsync(int categoryId);
    Task<IEnumerable<JobDTO>> GetJobsByCustomerAsync(int customerId);
    Task<IEnumerable<JobDTO>> GetJobsByLocationAsync(string city, int categoryId);

    // Using Views
    Task<IEnumerable<object>> GetActiveJobsWithBidsAsync();

    // Using Functions
    Task<int> CalculateJobComplexityAsync(decimal budget, string urgencyLevel, int requiredWorkers);
}
