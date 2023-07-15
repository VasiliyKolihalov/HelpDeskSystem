using Infrastructure.Extensions;
using Infrastructure.Middlewares;
using Users.WebApi.Repositories;
using Users.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddRabbitMqMessagePublisher("RabbitMqConfiguration");

string connectionString = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddTransient<IUsersRepository>(_ => new UsersRepository(connectionString));
builder.Services.AddTransient<UsersService>();

var app = builder.Build();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();

app.Run();