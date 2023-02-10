using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Infrastructure.DbModels;

namespace Pi.FileTransfer.Infrastructure;

internal class FileContext : DbContext
{
    public FileContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Destination> Destinations { get; set; }

    public DbSet<DbModels.File> Files { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Destination>()
            .HasKey(x => x.Folder);

        modelBuilder
            .Entity<DbModels.File>()
            .HasKey(x => x.Folder);
    }
}