# Users.Api

Users.Api is a microservice responsible for user data.

## Communications

This microservice publishes RabbitMQ messages which consumes [SupportTickets.Api](../SupportTickets.Api/README.md).
For RabbitMQ communication uses a standard client from [Infrastructure](../Infrastructure/README.md).

## Persistence

This microservice has own database `usersdb` in PostgreSQL.
Microservice uses [Microsoft.EntityFrameworkCore](https://github.com/dotnet/efcore) for generation sql query, mapping
results, and for migrations.

## Other technologies

For type mapping, this microservice uses [Automapper](https://github.com/AutoMapper/AutoMapper)
For auto describe API microservice uses [Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).