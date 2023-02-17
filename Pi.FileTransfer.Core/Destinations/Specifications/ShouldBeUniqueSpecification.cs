using Jdn.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Specifications;
internal class ShouldBeUniqueSpecification : ISpecification<DestinationDto>
{
    private readonly List<Destination> _destinations;

    public ShouldBeUniqueSpecification(List<Destination> destinations)
    {
        _destinations = destinations;
    }
    public Task<bool> IsSatisfiedBy(DestinationDto candiate)
        => Task.FromResult(_destinations.Any(x => string.Equals(x.Name, candiate.Name, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Address, candiate.Address,StringComparison.OrdinalIgnoreCase)));
}
