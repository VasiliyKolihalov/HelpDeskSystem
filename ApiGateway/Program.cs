using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);
WebApplication app = builder.Build();
await ConfigureMiddlewaresAsync(app);
app.Run();

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Configuration.AddOcelotWithSwaggerSupport(options => options.Folder = "Routes");
    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerForOcelot(builder.Configuration)
        .AddOcelot(builder.Configuration);
}

static async Task ConfigureMiddlewaresAsync(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerForOcelotUI();
    }

    await app.UseOcelot();
}