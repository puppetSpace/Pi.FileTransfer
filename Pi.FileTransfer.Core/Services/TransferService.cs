using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
public class TransferService
{
	private readonly IHttpClientFactory _clientFactory;
	private readonly ILogger<TransferService> _logger;

	public TransferService(IHttpClientFactory clientFactory, ILogger<TransferService> logger)
	{
		_clientFactory = clientFactory;
		_logger = logger;
	}

	public async Task Send(Destination destination, TransferSegment data)
	{
		_logger.LogDebug($"Sending segment {data.Sequencenumber} of file with id {data.FileId} from folder {data.FolderName} to {destination.Address}");
		var client = _clientFactory.CreateClient();
		var response = await client.PostAsJsonAsync($"{destination.Address}/api/file/segment", data);
		if (!response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			throw new TransferException($"Failed to transfer data to '{destination.Address}'. StatusCode: {response.StatusCode}. Content: {content}");
		}
	}

    public async Task SendReceipt(Destination destination, TransferReceipt transferReceipt)
    {
        _logger.LogDebug($"Sending receipt of file {transferReceipt.RelativePath} from folder {transferReceipt.FolderName} to {destination.Address}");
        var client = _clientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{destination.Address}/api/file/receipt", transferReceipt);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new TransferException($"Failed to send receipt to '{destination.Address}'. StatusCode: {response.StatusCode}. Content: {content}");
        }
    }
}
