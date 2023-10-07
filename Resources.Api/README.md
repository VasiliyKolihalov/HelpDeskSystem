# Resources.Api

Resources.Api is a microservice responsible for static media data, primarily images.

## Communication

This microservice communicates with [SupportTickets.Api](../SupportTickets.Api/README.md) using gRPS calls to get and
add images of messages. For gRPS communication uses [Grpc.Net](https://github.com/grpc/grpc-dotnet).

## Persistence

This microservice has own database `resourcesdb` in PostgreSQL.
Microservice uses [Microsoft.EntityFrameworkCore](https://github.com/dotnet/efcore) for generation sql query, mapping
results, and for migrations.

## Other technologies

For type mapping, microservice uses [Automapper](https://github.com/AutoMapper/AutoMapper)
