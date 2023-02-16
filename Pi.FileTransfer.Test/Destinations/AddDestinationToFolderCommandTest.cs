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
    }

    [Fact]
    public async Task Handle_FolderDoesNotExist()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.GetFolder(It.IsAny<string>())).Returns(Task.FromResult<Folder>(null));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination), CancellationToken.None);


        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _folderRepository.Verify(x => x.Add(_folder), Times.Never);
        Assert.Empty(_folder.Destinations);
    }

    [Fact]
    public async Task Handle_DestinationIsNull()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.GetFolder(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder)).Verifiable();
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, null), CancellationToken.None);


        _folderRepository.Verify(x => x.GetFolder(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FolderPathIsNullOrEmpty()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.GetFolder(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder)).Verifiable();
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(null, destination), CancellationToken.None);


        _folderRepository.Verify(x => x.GetFolder(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DestinationAdded()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folderRepository.Setup(x => x.GetFolder(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object);

        await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination), CancellationToken.None);


        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _folderRepository.Verify(x => x.Add(_folder), Times.Once);
        Assert.NotEmpty(_folder.Destinations);
        Assert.NotEmpty(_folder.Events);
    }

    [Fact]
    public async Task Handle_DuplicateDestinationNameAdded()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        _folder.AddDestination(destination);
        _folderRepository.Setup(x => x.GetFolder(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object);

        await Assert.ThrowsAsync<DestinationException>(async () => await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destination), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateDestinationAddressAdded()
    {
        var destination = new Destination(Guid.NewGuid(), "TestD", "https://");
        var destinationDupAddr = new Destination(Guid.NewGuid(), "TestD2", "https://");

        _folder.AddDestination(destination);

        _folderRepository.Setup(x => x.GetFolder(It.IsAny<string>())).Returns(Task.FromResult<Folder>(_folder));
        var commandHandler = new AddDestinationToFolderCommand.AddDestinationToFolderCommandHandler(_folderRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandHandler.Handle(new AddDestinationToFolderCommand(_folder.Name, destinationDupAddr), CancellationToken.None));
    }


}