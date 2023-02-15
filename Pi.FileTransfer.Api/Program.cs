using MediatR;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Destinations.Commands;
using Pi.FileTransfer.Core.Folders.Queries;
using Pi.FileTransfer.Core.Receives;
using Pi.FileTransfer.Core.Receives.Commands;
using Pi.FileTransfer.Core.Transfers.Queries;
using Pi.FileTransfer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddInfrastructure();
builder.Services.AddCore(builder.Configuration);

builder.Host.UseWindowsService(x =>
{
    x.ServiceName = "Pi FileTransfer";
});


var app = builder.Build();
app.AddMigration();


// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var apiEndpoint = app.MapGroup("api");

apiEndpoint.MapPost("/file/segment", async (Segment data, IMediator mediator) =>
{
    await mediator.Send(new StoreReceivedSegmentCommand(data));
});

apiEndpoint.MapPost("/file/receipt", async (Receipt data, IMediator mediator) =>
{
    await mediator.Send(new StoreReceivedReceiptCommand(data));
});

var managementEndPoint = app.MapGroup("management");
managementEndPoint.MapGet("/folders", async (IMediator mediator) =>
{
    return await mediator.Send(new GetFoldersQuery());
});
managementEndPoint.MapGet("/folders/{folder}/outgoing/details", async (IMediator mediator, string folder) =>
{
    return await mediator.Send(new GetOutgoingTransferDetailQuery(folder));
});
managementEndPoint.MapPost("/folders/{folder}/destination", async (IMediator mediator, Destination destination, string folder) =>
{
    await mediator.Send(new AddDestinationToFolderCommand(folder, destination));
});
managementEndPoint.MapDelete("/folders/{folder}/destination/{destination}", async (IMediator mediator, string folder, string destination) =>
{
    await mediator.Send(new DeleteDestinationFromFolderCommand(folder, destination));
});
managementEndPoint.MapGet("/folders/{folder}/incoming/details", async (IMediator mediator, string folder) =>
{
    return await mediator.Send(new GetIncomingTransferDetailQuery(folder));
});

app.Run();

