using Pi.FileTransfer.Core.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Interfaces;
public interface IDestinationRepository : IRepository
{
    Task<List<Destination>> GetAll();

    Task<Destination?> Get(Guid id);

    void Add(Destination destination);

    Task Delete(Guid destinationId);
}
