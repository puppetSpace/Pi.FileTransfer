using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Receives;
using Pi.FileTransfer.Core.Transfers;

namespace Pi.FileTransfer.Infrastructure;

internal class FileContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public FileContext(DbContextOptions options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Folder> Folders { get; set; }

    public DbSet<Destination> Destinations { get; set; }

    public DbSet<Core.Files.File> Files { get; set; }
    public DbSet<FailedReceipt> FailedReceipts{ get; set; }
    public DbSet<FailedSegment> FailedSegments{ get; set; }
    public DbSet<LastPosition> LastPositions{ get; set; }

    public DbSet<Receipt> Receipts { get; set; }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await _mediator.DispatchDomainEventsAsync(this);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Folder>()
            .HasKey(d => d.Id);

        modelBuilder
            .Entity<Folder>()
            .HasMany(x => x.Destinations)
            .WithMany();

        modelBuilder
            .Entity<Folder>()
            .HasMany(x => x.Files)
            .WithOne(x=>x.Folder);

        modelBuilder
            .Entity<Destination>()
            .HasKey(x => x.Id);

        modelBuilder
            .Entity<Core.Files.File>()
            .HasKey(x => x.Id);

        modelBuilder
            .Entity<Core.Files.File>()
            .HasOne(x => x.Folder)
            .WithMany(x => x.Files);
        
        modelBuilder
            .Entity<FailedReceipt>()
            .HasKey(x => new { FileId = x.File.Id, DestinationId = x.Destination.Id});

        modelBuilder
            .Entity<FailedReceipt>()
            .HasOne(x => x.Destination)
            .WithMany();

        modelBuilder
            .Entity<FailedReceipt>()
            .HasOne(x => x.File)
            .WithMany();

        modelBuilder
          .Entity<FailedSegment>()
          .HasKey(x => new { FileId = x.File.Id, DestinationId = x.Destination.Id });

        modelBuilder
            .Entity<FailedSegment>()
            .HasOne(x => x.Destination)
            .WithMany();

        modelBuilder
            .Entity<FailedSegment>()
            .HasOne(x => x.File)
            .WithMany();

        modelBuilder
            .Entity<FailedSegment>()
            .OwnsOne(x => x.Range);

        modelBuilder
            .Entity<LastPosition>()
            .HasKey(x => new { FileId =x.File.Id, DestinationId = x.Destination.Id }) ;

        modelBuilder
            .Entity<LastPosition>()
            .HasOne(x => x.Destination)
            .WithMany();
        modelBuilder
            .Entity<LastPosition>()
            .HasOne(x => x.File)
            .WithMany();

    }
}