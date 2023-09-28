using Infrastructure.Authentication.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Microsoft.EntityFrameworkCore;
using Users.Api.Constants;
using Users.Api.Repositories;
using Users.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);
WebApplication app = builder.Build();
app.TriggerEntityFrameworkMigrations<ApplicationContext>();
ConfigureMiddlewares(app);
app.Run();

static void ConfigureServices(WebApplicationBuilder builder)
{
    string connectionString = builder.Configuration.GetRequiredConnectionString("Default");
    builder.Services
        .AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString))
        .AddTransient<IUsersRepository, UsersRepository>();

    builder.Services
        .AddRabbitMqMessagePublisher(builder.Configuration.GetRequiredSection("RabbitMqOptions"))
        .AddAutoMapper(typeof(Program))
        .AddTransient<UsersService>();

    builder.Services
        .AddJwtAuthentication(builder.Configuration.GetRequiredSection("JwtAuthOptions"))
        .AddAuthorization(options => options
            .AddPoliticiansBasedOnJwtPermissions(PermissionNames.AllPolicyPermissions));

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(opt => opt.AddJwtSecurity())
        .AddControllers();
}


static void ConfigureMiddlewares(WebApplication app)
{
    app.UseHttpLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}