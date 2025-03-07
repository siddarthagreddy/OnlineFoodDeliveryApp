using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System.Text;


namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {

        private string connectionString = "Endpoint=sb://mangowebmessage.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ddItg+eHWKYp7QfZxfAX9oGMM0m72dj9q+ASbEOp878=";

        public async Task PublishMessage(object message, string topic_queue_Name)
        {
            await using var client = new ServiceBusClient(connectionString);

            ServiceBusSender serviceBusSender = client.CreateSender(topic_queue_Name);

            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            await serviceBusSender.SendMessageAsync(serviceBusMessage); 
            await client.DisposeAsync();
        }
    }
}
