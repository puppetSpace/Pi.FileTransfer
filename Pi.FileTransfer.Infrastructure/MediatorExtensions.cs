using MediatR;
using Pi.FileTransfer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure;
internal static class MediatorExtensions
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, FileContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Any());

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.Events)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}
