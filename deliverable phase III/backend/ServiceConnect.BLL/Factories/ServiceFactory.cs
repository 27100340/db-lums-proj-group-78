using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServiceConnect.BLL.Data;
using ServiceConnect.BLL.Interfaces;
using ServiceConnect.BLL.Services.LinqEF;
using ServiceConnect.BLL.Services.StoredProcedure;

namespace ServiceConnect.BLL.Factories;
/// used gpt help for this file
/// <summary>
/// Factory Pattern implementation for runtime BLL selection.
/// Allows switching between LINQ/Entity Framework and Stored Procedure implementations.
/// </summary>
public class ServiceFactory
{
    private readonly IConfiguration _configuration;
    private readonly ServiceConnectDbContext? _dbContext;
    private readonly BllType _bllType;

    public ServiceFactory(IConfiguration configuration, BllType bllType, ServiceConnectDbContext? dbContext = null)
    {
        _configuration = configuration;
        _bllType = bllType;
        _dbContext = dbContext;
    }

    public IJobService CreateJobService()
    {
        return _bllType == BllType.LinqEF
            ? new JobServiceEF(_dbContext ?? throw new InvalidOperationException("DbContext required for EF"))
            : new JobServiceSP(_configuration);
    }

    public IWorkerService CreateWorkerService()
    {
        return _bllType == BllType.LinqEF
            ? new WorkerServiceEF(_dbContext ?? throw new InvalidOperationException("DbContext required for EF"))
            : new WorkerServiceSP(_configuration);
    }

    public ICustomerService CreateCustomerService()
    {
        return _bllType == BllType.LinqEF
            ? new CustomerServiceEF(_dbContext ?? throw new InvalidOperationException("DbContext required for EF"))
            : new CustomerServiceSP(_configuration);
    }

    public IBidService CreateBidService()
    {
        return _bllType == BllType.LinqEF
            ? new BidServiceEF(_dbContext ?? throw new InvalidOperationException("DbContext required for EF"))
            : new BidServiceSP(_configuration);
    }

    public IBookingService CreateBookingService()
    {
        return _bllType == BllType.LinqEF
            ? new BookingServiceEF(_dbContext ?? throw new InvalidOperationException("DbContext required for EF"))
            : new BookingServiceSP(_configuration);
    }

    public IServiceCategoryService CreateServiceCategoryService()
    {
        return _bllType == BllType.LinqEF
            ? new ServiceCategoryServiceEF(_dbContext ?? throw new InvalidOperationException("DbContext required for EF"))
            : new ServiceCategoryServiceSP(_configuration);
    }
}

/// <summary>
/// Enum to specify which BLL implementation to use
/// </summary>
public enum BllType
{
    /// <summary>
    /// Use LINQ and Entity Framework Core for data access
    /// </summary>
    LinqEF,

    /// <summary>
    /// Use Stored Procedures with ADO.NET for data access
    /// </summary>
    StoredProcedure
}
