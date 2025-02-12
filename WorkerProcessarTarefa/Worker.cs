using Data.Db;
using Data.Enum;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace WorkerProcessarTarefa
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiDbContext db;
        private readonly string QueueName;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, ApiDbContext db)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            this.db = db;

            QueueName = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_QUEUE_NAME");

            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_PORT")),
                UserName = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_USER"),
                Password = Environment.GetEnvironmentVariable("CONFIG_RABBITMQ_PASSWORD"),
            };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _channel.QueueDeclareAsync(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, eventArgs) =>
            {
                bool sucesso = await ExecutarTarefas(eventArgs, cancellationToken);
                if (sucesso)
                {
                    await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                else
                {
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, false); // Rejeita a mensagem para DLQ
                }
            };

            await _channel.BasicConsumeAsync(queue: QueueName,
                                 autoAck: false,
                                 consumer: consumer, cancellationToken: cancellationToken);
        }

        private async Task<bool> ExecutarTarefas(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            int retryTime = Convert.ToInt32(Environment.GetEnvironmentVariable("CONFIG_POLLY_RETRY_TIME"));
            int retryCount = Convert.ToInt32(Environment.GetEnvironmentVariable("CONFIG_POLLY_RETRY_COUNT"));

            var retry = Policy
              .Handle<DivideByZeroException>()
              .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(retryTime));

            var body = eventArgs.Body.ToArray();
            var contratoTarefa = JsonSerializer.Deserialize<Data.Contratos.Tarefa>(Encoding.UTF8.GetString(body));

            var tarefa = db.Tarefas.Where(d => d.Id == contratoTarefa.Id).First();
            try
            {
                await retry.ExecuteAsync(async () =>
                {
                     await Execucao(contratoTarefa, tarefa, cancellationToken);
                });
            }
            catch (Exception ex)
            {
                tarefa.Status = StatusTarefa.Erro;
                await db.SaveChangesAsync(cancellationToken);
                return false;

            }
            return true;
        }

        private async Task<Data.Entidade.Tarefa> Execucao(Data.Contratos.Tarefa contratoTarefa, Data.Entidade.Tarefa tarefa, CancellationToken cancellationToken)
        {
            try
            {
                if (tarefa == null)
                {
                    //Se a tarefa ainda não existir eu crio ela nesse momento, essa definição deve fazert sentido de acordo com a regra de negocio
                    //Outras possibilidades é executar a tarefa ser registrar ela ou considerar um erro e não executar a tarefa
                    tarefa = new()
                    {
                        Id = contratoTarefa.Id,
                        Parametros = contratoTarefa.Parametros,
                        TipoTarefa = contratoTarefa.TipoTarefa.ToString(),
                        DataGravacao = DateTime.Now,
                        DataUltimaAlteracao = null,
                        Status = StatusTarefa.Pendente
                    };
                    db.Tarefas.Add(tarefa);
                }
                tarefa.Status = StatusTarefa.Processando;
                await db.SaveChangesAsync(cancellationToken);

                //TODO: AQUI IRÁ SER CHAMADA A LÓGICA PARA PROCESSAR A TAREFA DE ACORDO COM SEUS TIPOS E PARAMETROS
                #region Bloco para simular algumas tratativas e cenários de erros
                /*
                if (new Random().Next(0, 3) == 0) // Simulando aqui uma falha ocasional
                {
                    throw new Exception("Random error");
                }*/
                #endregion

                tarefa.Status = StatusTarefa.Concluido;
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                tarefa.Status = StatusTarefa.AguardandoReprocessamento;
                await db.SaveChangesAsync(cancellationToken);
                throw;
            }

            return tarefa;
        }
    }
}
