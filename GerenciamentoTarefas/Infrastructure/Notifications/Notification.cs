namespace GerenciamentoTarefas.Infrastructure.Notifications
{
    public class Notification
    {
        public Notification(string message, NotificationType type, string fieldName)
        {
            Message = message;
            Type = type;
            FieldName = fieldName;
        }

        public string Message { get; set; }
        public string FieldName { get; set; }
        public NotificationType Type { get; set; }
    }
}
