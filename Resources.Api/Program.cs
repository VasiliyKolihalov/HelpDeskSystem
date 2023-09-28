using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Resources.Api.GrpcServices;
using Resources.Api.Repositories;
using Resources.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);
WebApplication app = builder.Build();
app.TriggerEntityFrameworkMigrations<ApplicationContext>();
ConfigureMiddlewares(app);

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddGrpc();

    string connectionString = builder.Configuration.GetRequiredConnectionString("Default");
    builder.Services
        .AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString))
        .AddTransient<IImagesRepository, ImagesRepository>();

    builder.Services
        .AddAutoMapper(typeof(Program))
        .AddTransient<ImagesService>()
        .AddGrpc();
}

static void ConfigureMiddlewares(WebApplication app)
{
    app.MapGrpcService<GrpcImagesService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
    app.Run();
}