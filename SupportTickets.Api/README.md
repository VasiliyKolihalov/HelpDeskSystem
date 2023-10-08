# SupportTickets.Api

SupportTickets.Api is a microservice responsible for support tickets, their messages and solutions. This is the main
domain of the application.

## Communications

This microservice communicates with [Resources.Api](../Resources.Api/README.md) using gRPS calls to get and add images
of messages. For gRPS communication uses [Grpc.Net](https://github.com/grpc/grpc-dotnet).
Also, microservice consumes RabbitMQ messages from [Users.Api](../Users.Api/README.md) to duplicate
user data. This is necessary to reduce dependence on the
microservice [Users.Api](../Users.Api/README.md).
For RabbitMQ communication uses a standard client from [Infrastructure](../Infrastructure/README.md).

## Persistence

This microservice has own database `supporticketsdb` in PostgreSQL.
For mapping sql query results, this microservice uses micro orm [Dapper](https://github.com/DapperLib/Dapper).
For migration uses [FluentMigrator](https://github.com/fluentmigrator/fluentmigrator).

## Authentication

For jwt authentication, this microservice uses standard realisation
from [Infrastructure.Authentication](../Infrastructure.Authentication/README.md)

## Other technologies

For the implementation of business processes related to pending tasks, this microservice
uses [Hangfire](https://github.com/HangfireIO/Hangfire).
For type mapping, microservice uses [Automapper](https://github.com/AutoMapper/AutoMapper)
For auto describe API uses [Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).