using Data.Db;
using GerenciamentoTarefas.Infrastructure.Notifications;
using MediatR;

namespace GerenciamentoTarefas.Features.Tarefas.Query
{
    public class ConsultarTarefa
    {
        /// <summary>
        /// Informações de entrada para consultar a tarefa
        /// </summary>
        public class Query : IRequest<Result>
        {
            /// <summary>
            /// Campo para o id da tarefa
            /// </summary>
            public Guid Id { get; set; }
        }

        /// <summary>
        /// Informações da tarefa
        /// </summary>
        public class Result
        {
            /// <summary>
            /// id da tarefa
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// status atual da tarefa
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// data de criação da tarefa
            /// </summary>
            public DateTime DataCriacao { get; set; }

            /// <summary>
            /// data de ultima alteração na tarefa
            /// </summary>
            public DateTime? DataUltimaAlteracao { get; set; }
        }
        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly ApiDbContext db;
            private readonly NotificationContext notificationContext;

            public Handler(ApiDbContext db, NotificationContext notificationContext)
            {
                this.db = db;
                this.notificationContext = notificationContext;
            }
            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var tarefa = db.Tarefas.Where(d => d.Id == request.Id).FirstOrDefault();
                    if (tarefa == null)
                    {
                        return null;
                    }

                    return new Result
                    {
                        Id = tarefa.Id,
                        DataCriacao = tarefa.DataGravacao,
                        DataUltimaAlteracao = tarefa.DataUltimaAlteracao,
                        Status = tarefa.Status.ToString(),
                    };

                }
                catch (Exception ex)
                {
                    notificationContext.AddNotification("Ocorreu um erro interno, tente novamente mais tarde. Se o problema persistir entre em contato com o adminstrador do sistema. - " + ex.Message, NotificationType.ServerError);
                    return null;
                }
            }
        }
    }
}
