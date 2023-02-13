using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Infrastructure.DbModels;

namespace Pi.FileTransfer.Infrastructure;

internal class FileContext : DbContext
{
    private readonly IMediator _mediator;

    public FileContext(DbContextOptions options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Destination> Destinations { get; set; }

    public DbSet<DbModels.File> Files { get; set; }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await _mediator.DispatchDomainEventsAsync(this);

        return result;
    }

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