using Infrastructure.Middlewares;
using SupportTicketWebApi.Repositories;
using SupportTicketWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

string connectionString = builder.Configuration.GetConnectionString("Default")!;

builder.Services.AddTransient<ISupportTicketRepository>(_ => new SupportTicketRepository(connectionString));
builder.Services.AddTransient<SupportTicketService>();

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