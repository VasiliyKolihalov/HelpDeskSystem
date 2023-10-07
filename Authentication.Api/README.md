# Authentication

Authentication.Api is a microservice responsible for accounts, roles and permissions.

## Communication

This microservice communicates with [Users.Api](../Users.Api/README.md) by using REST, for keeping the rule one
account - one user.
Also, microservice publishes RabbitMQ messages which consumes [Notification.Api](../Notification.Api/README.md).
For rabbit mq communication uses a standard client from [Infrastructure](../Infrastructure/README.md).

## Persistence

This microservice has own database `accountsdb` in postgres.
For mapping sql query results, this microservice uses micro orm [Dapper](https://github.com/DapperLib/Dapper).
For migration uses [FluentMigrator](https://github.com/fluentmigrator/fluentmigrator).

## Authentication

For authentication, this microservice uses standard realisation
from [Infrastructure.Authentication](../Infrastructure.Authentication/README.md)

## Other technologies

For hash passwords, this microservice uses [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net).
For type mapping, microservice uses [Automapper](https://github.com/AutoMapper/AutoMapper)
For auto describe API uses [Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).