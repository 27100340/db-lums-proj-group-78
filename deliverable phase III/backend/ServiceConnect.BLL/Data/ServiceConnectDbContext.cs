using Microsoft.EntityFrameworkCore;
using ServiceConnect.Models.Entities;

namespace ServiceConnect.BLL.Data;

public class ServiceConnectDbContext : DbContext
{
    public ServiceConnectDbContext(DbContextOptions<ServiceConnectDbContext> options)
        : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Worker> Workers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;
    public DbSet<WorkerSkill> WorkerSkills { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<Bid> Bids { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<JobAttachment> JobAttachments { get; set; } = null!;
    public DbSet<WorkerAvailability> WorkerAvailabilities { get; set; } = null!;
    public DbSet<WorkerServiceArea> WorkerServiceAreas { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names to match SQL Server
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Worker>().ToTable("Workers");
        modelBuilder.Entity<Customer>().ToTable("Customers");
        modelBuilder.Entity<ServiceCategory>().ToTable("ServiceCategories");
        modelBuilder.Entity<WorkerSkill>().ToTable("WorkerSkills");
        modelBuilder.Entity<Job>().ToTable("Jobs");
        modelBuilder.Entity<Bid>().ToTable("Bids");
        modelBuilder.Entity<Booking>().ToTable("Bookings");
        modelBuilder.Entity<Review>().ToTable("Reviews");
        modelBuilder.Entity<JobAttachment>().ToTable("JobAttachments");
        modelBuilder.Entity<WorkerAvailability>().ToTable("WorkerAvailability");
        modelBuilder.Entity<WorkerServiceArea>().ToTable("WorkerServiceAreas");
        modelBuilder.Entity<Notification>().ToTable("Notifications");

        // Configure relationships
        modelBuilder.Entity<Worker>()
            .HasOne(w => w.User)
            .WithOne(u => u.Worker)
            .HasForeignKey<Worker>(w => w.WorkerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        // Disable cascading deletes to match SQL Server constraints
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Configure indexes (matching Phase 2 SQL)
        modelBuilder.Entity<Job>().HasIndex(j => j.CustomerID);
        modelBuilder.Entity<Job>().HasIndex(j => j.CategoryID);
        modelBuilder.Entity<Job>().HasIndex(j => j.Status);
        modelBuilder.Entity<Job>().HasIndex(j => j.PostedDate);
        modelBuilder.Entity<Bid>().HasIndex(b => b.JobID);
        modelBuilder.Entity<Bid>().HasIndex(b => b.WorkerID);
        modelBuilder.Entity<Bid>().HasIndex(b => b.Status);
        modelBuilder.Entity<Booking>().HasIndex(b => b.JobID);
        modelBuilder.Entity<Booking>().HasIndex(b => b.WorkerID);
        modelBuilder.Entity<Booking>().HasIndex(b => b.Status);
        modelBuilder.Entity<Review>().HasIndex(r => r.BookingID);
        modelBuilder.Entity<User>().HasIndex(u => u.Email);
        modelBuilder.Entity<Worker>().HasIndex(w => w.City);
        modelBuilder.Entity<WorkerSkill>().HasIndex(ws => ws.WorkerID);
        modelBuilder.Entity<Notification>().HasIndex(n => n.UserID);
    }
}
