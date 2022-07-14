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

	public TransferService(IHttpClientFactory clientFactory)
	{
		_clientFactory = clientFactory;
	}

	public async Task Send(Destination destination, TransferSegment data)
	{
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
        var client = _clientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{destination.Address}/api/file/receipt", transferReceipt);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new TransferException($"Failed to send receipt to '{destination.Address}'. StatusCode: {response.StatusCode}. Content: {content}");
        }
    }
}
