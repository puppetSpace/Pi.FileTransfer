using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations.Exceptions;
using Pi.FileTransfer.Core.Destinations.Specifications;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Commands;
public class AddDestinationCommand : IRequest<Destination>
{
    public AddDestinationCommand(DestinationDto destinationDto)
    {
        DestinationDto = destinationDto;
    }

    public DestinationDto DestinationDto { get; }


    internal class AddDestinationCommandHandler : IRequestHandler<AddDestinationCommand, Destination>
    {
        private readonly IDestinationRepository _destinationRepository;
        private readonly ILogger<AddDestinationCommand> _logger;

        public AddDestinationCommandHandler(IDestinationRepository destinationRepository, ILogger<AddDestinationCommand> logger)
        {
            _destinationRepository = destinationRepository;
            _logger = logger;
        }

        public async Task<Destination> Handle(AddDestinationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Adding destination {request.DestinationDto.Name} with address {request.DestinationDto.Address}");
            var allDestinations = await _destinationRepository.GetAll();
            
            if ( !await new ShouldBeUniqueSpecification(allDestinations).IsSatisfiedBy(request.DestinationDto)) 
            {
                throw new DestinationException($"Destination with name {request.DestinationDto.Name} or address {request.DestinationDto.Address} already exists");
            }

            var destination = new Destination(Guid.NewGuid(),request.DestinationDto.Name,request.DestinationDto.Address);
            _destinationRepository.Add(destination);
            await _destinationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return destination;
        }
    }
}
