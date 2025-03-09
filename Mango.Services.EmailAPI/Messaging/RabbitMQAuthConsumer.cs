using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;

            // Initialize connection asynchronously without blocking constructor
            Task.Run(async () => await InitializeConnectionAsync());
        }

        private async Task InitializeConnectionAsync()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    Password = "guest",
                    UserName = "guest"
                };

                // Create Connection (Retry if fails)
                int retryCount = 5;
                while (_connection == null && retryCount > 0)
                {
                    try
                    {
                        _connection = await factory.CreateConnectionAsync();
                        Console.WriteLine("RabbitMQ Connection established.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Retrying RabbitMQ connection... ({5 - retryCount} attempts left) - {ex.Message}");
                        await Task.Delay(2000); // Wait before retrying
                    }
                    retryCount--;
                }

                if (_connection == null)
                {
                    Console.WriteLine("❌ Failed to establish RabbitMQ connection after retries!");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RabbitMQ initialization failed: {ex.Message}");
            }
        }

        private async Task CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    Password = "guest",
                    UserName = "guest"
                };

                _connection = await factory.CreateConnectionAsync();
                Console.WriteLine("✅ RabbitMQ Connection & Channel Created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RabbitMQ connection failed: {ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            stoppingToken.ThrowIfCancellationRequested();
            if (_connection == null)
            {
                Console.WriteLine("🔄 Reconnecting to RabbitMQ...");
                await CreateConnection();
            }

            _channel = await _connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                string email = JsonConvert.DeserializeObject<string>(content);
                await HandleMessage(email);

                await _channel.BasicAckAsync(ea.DeliveryTag, false); // Use async acknowledgment
            };

            var registerUser = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
            await _channel.BasicConsumeAsync(registerUser, false, consumer);
        }

        private async Task HandleMessage(string email)
        {
            await _emailService.RegisterUserEmailAndLog(email);
        }
    }
}
