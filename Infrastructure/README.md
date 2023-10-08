# Infrastructure

Infrastructure is a local library that contains abstractions and services that almost all other solutions use.

Several important things this solution contains:

* Universal exceptions for services.
* Extensions for work with required configuration values and binds
* Extensions for using [Dapper transaction](https://github.com/zzzprojects/Dapper.Transaction)
* Extensions for using several database migration providers
* Extension for using http client with [Polly](https://github.com/App-vNext/Polly)
* Global exception handler middleware
* RabbitMQ publisher
* RabbitMQ consumer



