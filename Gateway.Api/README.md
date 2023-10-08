# Gateway.Api

Gateway.Api is a web service that implements the gateway pattern. For implementation this pattern
using [Ocelot](https://github.com/ThreeMammals/Ocelot).

## Communications

This service communicates
with[SupportTickets.Api](../SupportTickets.Api/README.md),[Authentication.Api](../Authentication.Api/README.md)
and[Users.Api](../Users.Api/README.md) by using REST, to coordinate requests for these downstream microservice

## Other technologies

For describe downstream microservices API
uses [Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
with [SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot).