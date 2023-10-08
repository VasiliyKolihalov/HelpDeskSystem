# Notification.Api

Notification.Api is a microservice responsible for user notifications, primarily email notifications.
This is not ASP.NET Core application like other microservices, this is the hosted worker service.

## Communications

This microservice consumes RabbitMQ messages from [Authentication.Api](../Authentication.Api/README.md) to notify user
about account actions.
For RabbitMQ communication uses a standard client from [Infrastructure](../Infrastructure/README.md).

## Other technologies

For sending emails, this microservice uses [MailKit](https://github.com/jstedfast/MailKit).
For render html templates, microservice uses [Scriban](https://github.com/scriban/scriban)