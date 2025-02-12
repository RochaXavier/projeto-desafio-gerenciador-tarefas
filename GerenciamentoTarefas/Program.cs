using Data.Db;
using FluentValidation;
using GerenciamentoTarefas.Infrastructure.Behaviors;
using GerenciamentoTarefas.Infrastructure.Notifications;
using GerenciamentoTarefas.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region MVC            

builder.Services.AddControllers(config =>
{
    config.Filters.Add(typeof(NotificationFilter));
}).AddJsonOptions(config =>
{
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
#endregion MVC


#region Infrastructure
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

AssemblyScanner.FindValidatorsInAssembly(Assembly.GetExecutingAssembly())
    .ForEach(validator => builder.Services.AddScoped(validator.InterfaceType, validator.ValidatorType));

builder.Services.AddScoped<NotificationContext>();

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(NotificationValidationBehavior<,>));

builder.Services.AddSingleton<ISenderMessageService, SenderMessageService>();
#endregion Infrastructure

#region Documentation

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gerencimento de tarefas", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, true);
});

builder.Services.ConfigureSwaggerGen(c => c.CustomSchemaIds(x => x.FullName.Replace("+",".")));

#endregion Documentation


#region Database
//DbContext
builder.Services.AddDbContext<ApiDbContext>(options => options.UseMongoDB(
    connectionString: Environment.GetEnvironmentVariable("CONFIG_MONGODB_CONNECTION_STRING"),
    databaseName: Environment.GetEnvironmentVariable("CONFIG_MONGODB_DATABASE_NAME")));
#endregion Database

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    //c.SwaggerEndpoint("GerenciamentoTarefas.xml", "Aplicação de Processamento de Tarefas");
    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "myappname v1"); 
    //c.RoutePrefix = "/swagger";
});

app.Run();
