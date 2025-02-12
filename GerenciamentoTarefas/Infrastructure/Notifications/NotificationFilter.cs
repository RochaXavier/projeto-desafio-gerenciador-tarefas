using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GerenciamentoTarefas.Infrastructure.Notifications
{
    public class NotificationFilter : IAsyncResultFilter
    {
        private readonly NotificationContext notificationContext;
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly Dictionary<NotificationType, HttpStatusCode> notificationTypeToStatusCode = new Dictionary<NotificationType, HttpStatusCode>
        {
            { NotificationType.Success, HttpStatusCode.OK },
            { NotificationType.ResourceNotFound, HttpStatusCode.NotFound },
            { NotificationType.InvalidArguments, HttpStatusCode.BadRequest },
            { NotificationType.UnauthenticatedAccess, HttpStatusCode.Unauthorized },
            { NotificationType.ForbiddenAccess, HttpStatusCode.Forbidden },
            { NotificationType.ServerError, HttpStatusCode.InternalServerError },
        };

        public NotificationFilter(NotificationContext notificationContext, IOptions<JsonSerializerOptions> jsonSerializer)
        {
            this.notificationContext = notificationContext;
            this.jsonSerializerOptions = jsonSerializer.Value;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (notificationContext.HasNotifications)
            {
                context.HttpContext.Response.ContentType = "application/json";

                var notificationGroup = notificationContext.Notifications.GroupBy(x => x.Type).First();

                var statusCode = notificationTypeToStatusCode.GetValueOrDefault(notificationGroup.Key, HttpStatusCode.BadRequest);
                context.HttpContext.Response.StatusCode = (int)statusCode;

                var notificationsJson = JsonSerializer.Serialize(notificationGroup, jsonSerializerOptions);

                await context.HttpContext.Response.WriteAsync(notificationsJson);

                return;
            }

            await next();
        }
    }
}
