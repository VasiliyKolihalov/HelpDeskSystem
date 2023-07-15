using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddOcelot("Routes", builder.Environment);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
await app.UseOcelot();
app.Run();