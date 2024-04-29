using Azure.Messaging.ServiceBus;
using Receiver.Entities;
using Receiver.Repo;
using System.Diagnostics;

namespace Receiver.Handler;

public class ServiceBusHandler
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusRepo _repo;

    public ServiceBusHandler(string connectionString, string queueSubscription, string queueConfirmation, ServiceBusRepo repo)
    {
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueConfirmation);
        _processor = _client.CreateProcessor(queueSubscription);

        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
        _repo = repo;
    }

    public async Task StartSubscribing()
    {
        await _processor.StartProcessingAsync();
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Debug.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        try
        {
            string message = args.Message.Body.ToString();

            if (!string.IsNullOrEmpty(message))
            {
                var entity = new SubscribeEntity
                {
                    Email = message,
                    SubscriptionDate = DateTime.UtcNow,
                };
                var result = await _repo.CreateAsync(entity);
                if (result is not null)
                {
                    await PublishAsync(message);
                }
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        { Debug.WriteLine($"Detta är felet:: {ex}"); }
    }

    public async Task PublishAsync(string message, string messageType = null!)
    {
        var payload = new ServiceBusMessage(message);
        if (messageType is not null)
        {
            payload.ApplicationProperties.Add("messageType", messageType);
        }

        await _sender.SendMessageAsync(payload);
    }
}
