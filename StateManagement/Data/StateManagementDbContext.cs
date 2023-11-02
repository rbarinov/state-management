using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StateManagement.Data.Entities;

namespace StateManagement.Data;

public class StateManagementDbContext : DbContext
{
    public StateManagementDbContext(DbContextOptions<StateManagementDbContext> options)
        : base(options)
    {
    }

    public required DbSet<StreamDto> Streams { get; set; }
    public required DbSet<EventDto> Events { get; set; }
    public required DbSet<StateDto> States { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
