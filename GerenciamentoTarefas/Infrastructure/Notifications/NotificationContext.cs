using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace GerenciamentoTarefas.Infrastructure.Notifications
{
    public class NotificationContext
    {
        private readonly List<Notification> _notifications;
        public IReadOnlyList<Notification> Notifications => _notifications;
        public bool HasNotifications => _notifications.Any();

        public NotificationContext()
        {
            _notifications = new List<Notification>();
        }

        public void AddNotification(string message, NotificationType notificationType, string field = "errors")
            => _notifications.Add(new Notification(message, notificationType, field));

        public void AddDefaultResourseNotFoundNotification()
            => AddNotification("Not Found", notificationType: NotificationType.ResourceNotFound);

        public void AddValidationNotifications(ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
                this.AddNotification(error.ErrorMessage, NotificationType.InvalidArguments, error.PropertyName);
        }

        public void Reset()
        {
            _notifications.Clear();
        }
    }
}