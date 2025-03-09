using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace Mango.Services.AuthAPI.RabbitMQSender
{
    public class RabbitMQAuthMessageSender : IRabbitMQAuthMessageSender
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;
        private IChannel _channel; // ✅ Keep a persistent channel

        public RabbitMQAuthMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _username = "guest";

            Task.Run(async () => await CreateConnection()); // 🔹 Ensure connection is established on startup
        }

        public async Task SendMessage(object message, string queueName)
        {
            if (!await ConnectionExists())
            {
                Console.WriteLine("❌ No RabbitMQ connection. Message not sent.");
                return;
            }

            try
            {
                _channel = await _connection.CreateChannelAsync(); // ✅ Create persistent channel

                // ✅ Ensure the queue exists before publishing
                await _channel.QueueDeclareAsync(queueName, false, false, false, null);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // ✅ Publish message to RabbitMQ
                await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
                Console.WriteLine($"✅ Message sent to queue: {queueName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send message: {ex.Message}");
            }
        }

        private async Task CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _username
                };

                _connection = await factory.CreateConnectionAsync();
                Console.WriteLine("✅ RabbitMQ Connection & Channel Created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RabbitMQ connection failed: {ex.Message}");
            }
        }

        private async Task<bool> ConnectionExists()
        {
            if (_connection == null || !_connection.IsOpen || _channel == null)
            {
                Console.WriteLine("🔄 Reconnecting to RabbitMQ...");
                await CreateConnection();
            }
            return _connection != null && _connection.IsOpen && _channel != null;
        }
    }
}
