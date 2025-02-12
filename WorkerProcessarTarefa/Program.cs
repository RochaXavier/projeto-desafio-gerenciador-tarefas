using Data.Db;
using Microsoft.EntityFrameworkCore;
using WorkerProcessarTarefa;

var builder = Host.CreateApplicationBuilder(args);

#region Database
builder.Services.AddDbContext<ApiDbContext>(options => options.UseMongoDB(
    connectionString: Environment.GetEnvironmentVariable("CONFIG_MONGODB_CONNECTION_STRING"),
    databaseName: Environment.GetEnvironmentVariable("CONFIG_MONGODB_DATABASE_NAME")), ServiceLifetime.Singleton);
#endregion

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
