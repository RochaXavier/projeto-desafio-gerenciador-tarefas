using FluentValidation;
using GerenciamentoTarefas.Infrastructure.Notifications;
using MediatR;

namespace GerenciamentoTarefas.Infrastructure.Behaviors
{
    public class NotificationValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;
        private readonly NotificationContext notificationContext;

        public NotificationValidationBehavior(IEnumerable<IValidator<TRequest>> validators, NotificationContext notificationContext)
        {
            this.validators = validators;
            this.notificationContext = notificationContext;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var failures = validators
                                .Select(v => v.Validate(request))
                                .Where(f => !f.IsValid)
                                .ToList();

            if (failures.Any())
            {
                failures.ForEach(notificationContext.AddValidationNotifications);
                return default;
            }

            return await next();
        }
    }
}