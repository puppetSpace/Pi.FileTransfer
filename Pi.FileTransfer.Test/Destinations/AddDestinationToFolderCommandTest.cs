using Moq;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Destinations.Commands;
using Pi.FileTransfer.Core.Destinations.Exceptions;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Test.Destinations;

public class AddDestinationToFolderCommandTest
{
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IFolderRepository> _folderRepository;
    private Mock<IDestinationRepository> _destinationRepository;
    private Folder _folder = new Folder(Guid.NewGuid(), "Test", @"D:\Test", new(), new());

    public AddDestinationToFolderCommandTest()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1))
            .Verifiable();

        _folderRepository = new Mock<IFolderRepository>();
        _folderRepository.SetupGet(x => x.UnitOfWork).Returns(_unitOfWork.Object);
        _folderRepository.Setup(x => x.Add(It.IsAny<Folder>())).Verifiable();

        _destinationRepository = new Mock<IDestinationRepository>();
        
    }

    [Fact]
    public async Task Handle_FolderDoesNotExist()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(null));
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult(destination));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object,_destinationRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination.Id), CancellationToken.None);


        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _folderRepository.Verify(x => x.Add(_folder), Times.Never);
        Assert.Empty(_folder.Destinations);
    }

    [Fact]
    public async Task Handle_DestinationIsNull()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder)).Verifiable();
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult<Destination>(destination)).Verifiable();
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object, _destinationRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, Guid.Empty), CancellationToken.None);


        _folderRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
        _destinationRepository.Verify(x => x.Get(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DestinationDoesNotExists()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder)).Verifiable();
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult<Destination>(null)).Verifiable();
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object, _destinationRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination.Id), CancellationToken.None);


        _destinationRepository.Verify(x => x.Get(It.IsAny<Guid>()), Times.Once);
        _folderRepository.Verify(x => x.Add(_folder), Times.Never);
    }

    [Fact]
    public async Task Handle_FolderPathIsNullOrEmpty()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder)).Verifiable();
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult<Destination>(destination));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object, _destinationRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(null, destination.Id), CancellationToken.None);


        _folderRepository.Verify(x => x.Get(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DestinationAdded()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder));
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult<Destination>(destination));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object,_destinationRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination.Id), CancellationToken.None);


        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _folderRepository.Verify(x => x.Add(_folder), Times.Once);
        Assert.NotEmpty(_folder.Destinations);
        Assert.NotEmpty(_folder.Events);
        Assert.Contains(_folder.Destinations, x =>x.Id ==  destination.Id);
    }

    [Fact]
    public async Task Handle_DuplicateDestinationNameAdded()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folder.AddDestination(destination);
        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder));
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult<Destination>(destination));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object, _destinationRepository.Object);

        await Assert.ThrowsAsync<DestinationException>(async () => await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateDestinationAddressAdded()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        var destinationDupAddr = new Destination(Guid.NewGuid(), "TestD2", "https://");

        _folder.AddDestination(destination);

        _folderRepository.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder));
        _destinationRepository.Setup(x => x.Get(It.IsAny<Guid>())).Returns(Task.FromResult<Destination>(destination));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object, _destinationRepository.Object);

        await Assert.ThrowsAsync<DestinationException>(async () => await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destinationDupAddr.Id), CancellationToken.None));
    }


}