using HelpDeskSystem.Middlewares;
using HelpDeskSystem.Repositories;
using HelpDeskSystem.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
RegisterServices(builder);
WebApplication app = builder.Build();
RegisterMiddlewares(app);
app.Run();

static void RegisterServices(WebApplicationBuilder builder)
{
    string connectionString = builder.Configuration.GetConnectionString("Default");

    builder.Services
        .AddTransient<IApplicationContext>(_ => new ApplicationContext(connectionString))
        .AddAutoMapper(typeof(Program))
        .AddTransient<SupportTicketsService>()
        .AddControllers();
}

static void RegisterMiddlewares(WebApplication app)
{
    app
        .UseHttpLogging()
        .UseMiddleware<ErrorHandlerMiddleware>();

    app.MapControllers();
}