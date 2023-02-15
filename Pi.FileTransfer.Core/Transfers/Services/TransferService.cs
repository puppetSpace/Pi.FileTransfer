using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Receives;
using Pi.FileTransfer.Core.Transfers.Exceptions;
using System.Net.Http.Json;

namespace Pi.FileTransfer.Core.Transfers.Services;
public class TransferService
{
    public const string HttpClientName = "Transfer";
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<TransferService> _logger;

    public TransferService(IHttpClientFactory clientFactory, ILogger<TransferService> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task Send(Destination destination, Segment data)
    {
        _logger.SendSegment(data.Sequencenumber, data.FileId, data.FolderName, destination.Address);
        var client = _clientFactory.CreateClient(HttpClientName);
        var response = await client.PostAsJsonAsync($"{destination.Address}/api/file/segment", data);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new TransferException($"Failed to transfer data to '{destination.Address}'. StatusCode: {response.StatusCode}. Content: {content}");
        }
    }

    public async Task SendReceipt(Destination destination, Receipt transferReceipt)
    {
        _logger.SendReceipt(transferReceipt.RelativePath, destination.Address);
        var client = _clientFactory.CreateClient(HttpClientName);
        var response = await client.PostAsJsonAsync($"{destination.Address}/api/file/receipt", transferReceipt);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new TransferException($"Failed to send receipt to '{destination.Address}'. StatusCode: {response.StatusCode}. Content: {content}");
        }
    }
}
