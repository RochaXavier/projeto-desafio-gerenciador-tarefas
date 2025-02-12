using Data.Entidade;
using Data.Enum;
using FluentValidation;
using Data.Db;
using GerenciamentoTarefas.Services;
using MediatR;
using GerenciamentoTarefas.Infrastructure.Notifications;

namespace GerenciamentoTarefas.Features.Tarefas.Command
{
    public class CriarTarefa
    {
        /// <summary>
        /// Dados de entrada para criação da nova tarefa
        /// </summary>
        public class Command : IRequest<Result>
        {
            /// <summary>
            /// campo Enum para o tipo de tarefa
            /// </summary>
            public TipoTarefas TipoTarefa { get; set; }
            /// <summary>
            /// lista de chave e valor para os parametros configuraveis 
            /// </summary>
            public Dictionary<string, string> Parametros { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(d => d.TipoTarefa).IsInEnum().WithMessage("Valor para tipoTarefa inválido");
            }
        }

        /// <summary>
        /// Retorno da criação da tarefa
        /// </summary>
        public class Result
        {
            /// <summary>
            /// id da tarefa criada
            /// </summary>
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly ApiDbContext db;
            private readonly ISenderMessageService messageService;
            private readonly NotificationContext notificationContext;

            public Handler(ApiDbContext db, ISenderMessageService messageService, NotificationContext notificationContext)
            {
                this.db = db;
                this.messageService = messageService;
                this.notificationContext = notificationContext;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    Tarefa tarefa = new()
                    {
                        Id = Guid.NewGuid(),
                        Parametros = request.Parametros,
                        TipoTarefa = request.TipoTarefa.ToString(),
                        DataGravacao = DateTime.Now,
                        DataUltimaAlteracao = null,
                        Status = StatusTarefa.Pendente
                    };
                    db.Tarefas.Add(tarefa);

                    await db.SaveChangesAsync(cancellationToken);

                    await messageService.EnqueueAsync(new Data.Contratos.Tarefa()
                    {
                        Id = tarefa.Id,
                        Parametros = request.Parametros,
                        TipoTarefa = request.TipoTarefa
                    });

                    return new Result() { Id = tarefa.Id };

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
