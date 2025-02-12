using System.Text;
using System.Text.Json;
using Data.Contratos;
using RabbitMQ.Client;

namespace GerenciamentoTarefas.Services
{
    public class SenderMessageService : ISenderMessageService
    {
        private readonly ConnectionFactory factory;
        private readonly IConnection connection;
        private readonly string QueueName;
        public SenderMessageService()
        {
            QueueName = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_QUEUE_NAME");

            factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_PORT")),
                UserName = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_USER"),
                Password = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_PASSWORD"),
            };
            connection = factory.CreateConnectionAsync().Result;

        }

        public async Task<bool> EnqueueAsync(Tarefa tarefa)
        {
            try
            {
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var json = JsonSerializer.Serialize(tarefa);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: QueueName, body: body);

                //Console.WriteLine($"Mensagem enviada - id {tarefa.Id}");
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        

    }
}