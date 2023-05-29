## GraphQL ASP.NET Test Framework

### > [General Documentation](https://graphql-aspnet.github.io)

### > [Test Framework Documentation](https://graphql-aspnet.github.io/docs/development/unit-testing)

GraphQL ASP.NET is a fully featured graphql library that utilizes a controller/action programming model similar to ASP.NET. This library is a 1st party test framework to allow you quickly spin up a test server and test your controller methods in an isolated manner. 


```csharp 
// Excute a simple Test against a controller method
public async Task WhenGivenTwoNumbers_TheSumIsReturned()
{
    // **Arrange**
    var expectedOutput = @"{ 
        ""data"" : {
            ""addTwoNumbers"": 13
        }
    }"

    // generate a 'server' of a schema with one controller
    var server = new TestServerBuilder()
                    .AddController<CalculatorController>()
                    .Build();

    // build a request to send to the server
    var queryBuilder = server.CreateQueryContextBuilder();
    queryBuilder.AddQueryText("query { addTwoNumbers(num1: 5, num2: 8) }");

    // **Act**
    var jsonResult = await server.RenderResult(queryBuilder);

    // **Assert**
    CommonAssertions.AreEqualJsonStrings(expectedOutput, jsonResult);    
}
```

### Supported Features
* Setup specific authentication and authorization scenarios to test security.
* Mock and inject any dependencies for your controllers.
* Interrogate the `IQueryExecutionResult` object for specific data, messages, metrics or thrown exceptions.
* Render the resultant json document that would be sent to the requestor.

Read the [documentation](https://graphql-aspnet.github.io/docs/development/unit-testing) for full details

