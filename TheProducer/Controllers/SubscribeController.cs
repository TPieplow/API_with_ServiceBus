using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using System.Diagnostics;

namespace TheProducer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscribeController(ServiceBusSender serviceBusSender, ServiceBusClient serviceBusClient) : ControllerBase
{
    private readonly ServiceBusSender _serviceBusSender = serviceBusSender;
    private readonly ServiceBusClient _client = serviceBusClient;

    [HttpPost]
    public async Task<IActionResult> Subscribe(string email)
    {
        try
        {
            if (!string.IsNullOrEmpty(email))
            {
                var encodedMsg = new ServiceBusMessage(email);
                await _serviceBusSender.SendMessageAsync(encodedMsg);

                var receiver = _client.CreateReceiver("confirmation");
                ServiceBusReceivedMessage responseMessage = await receiver.ReceiveMessageAsync();

                await receiver.CompleteMessageAsync(responseMessage);

                return Created($"Successfully subscribed {responseMessage.Body}", null);
            }
        }
        catch (Exception ex) { Debug.WriteLine($"Error:: {ex.Message}");  }
        return BadRequest();
    }
}
