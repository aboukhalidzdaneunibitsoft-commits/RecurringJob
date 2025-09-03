using Microsoft.EntityFrameworkCore;
using RecurringJob.API.Entitys;

namespace RecurringJob.API.Databases;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions): base(dbContextOptions)
    {
        
    }
   public DbSet<TimeTrigger> TimeTriggers { get; set; }
    public DbSet<TriggerExecutionLog> TriggerExecutionLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimeTrigger>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NextExecutionTime);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<TriggerExecutionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Trigger)
                  .WithMany()
                  .HasForeignKey(e => e.TriggerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TriggerId);
            entity.HasIndex(e => e.ExecutedAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}

