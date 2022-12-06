## Performance Load Test Projects

The load test projects represent the various components used in setting up a load test environment. See the documentation for details on the various tests that can be executed.

### Project: graphql-aspnet-load-console
A console application used to subscribe to subscription events during some load tests. This project also provides a rudimentary way to run a moderate number of client requests against the load server without having to execute a full blown jmeter test plan.

### Project: graphql-aspnet-load-server
The server instance containing the controllers (REST and GraphQL) that all load tests are run against.

### Project: graphql-aspnet-load-models
The shared set of model objects used on the load console and the server. Allows for easy sharing of objects for deserialization and response validation.

### Folder: graphql-aspnet-load-jmeter
The set of preconfigured jmeter test scripts used to generate load on the server instance for different tests.