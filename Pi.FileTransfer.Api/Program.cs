using MediatR;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Queries;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddInfrastructure();
builder.Services.AddCore(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var apiEndpoint = app.MapGroup("api");

apiEndpoint.MapPost("/file/segment", async (TransferSegment data, DataStore fsStore) =>
{
    await fsStore.StoreReceivedSegment(data);
});

apiEndpoint.MapPost("/file/receipt", async (TransferReceipt data, DataStore fsStore) =>
{
    await fsStore.StoreReceivedReceipt(data);
});

var managementEndPoint = app.MapGroup("management");
managementEndPoint.MapGet("/folders", async (IMediator mediator) =>
{
    return await mediator.Send(new GetFoldersQuery());
});
managementEndPoint.MapGet("/folders/{name}/outgoing/details", async (IMediator mediator, string name) =>
{
    return await mediator.Send(new GetOutgoingTransferDetailQuery(name));
});
managementEndPoint.MapGet("/folders/{name}/incoming/transferdetails", async (IMediator mediator, string name) =>
{
    return await mediator.Send(new GetIncomingTransferDetailQuery(name));
});

app.Run();

