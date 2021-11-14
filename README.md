## GraphQL ASP.NET (beta)

### Documentation: [https://graphql-aspnet.github.io](https://graphql-aspnet.github.io)

> Target Specification: **netstandard2.0**
>
> Supported Runtimes: _.NET 4.7.2+_, _.NET Core 3.1+_, _.NET 5_, _.NET 6_

GraphQL ASP.NET is a fully featured graphql library that utilizes a controller/action programming model familiar to ASP.NET MVC developers. Instead of focusing on schemas and mapping resolvers, the focus on controllers and models. GraphQL ASP.NET will automatically generate the schema to match your code.

| Recent Builds |                                                                                                                                                                                                                                                                    |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Development   | [![Develop Branch Build Status](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_apis/build/status/CI%20%26%20Deployment%20Build?branchName=develop)](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_build/latest?definitionId=4&branchName=develop) |
| Master        | [![Master Branch Build Status](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_apis/build/status/CI%20%26%20Deployment%20Build?branchName=master)](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_build/latest?definitionId=4&branchName=master)    |

#### Example Usage:

**This Controller**

```csharp
// BakeryController.cs
[GraphRoute("groceryStore/bakery")]
public class BakeryController : GraphController
{
    // Automatic "scoped" dependency injection
    public BakeryController(IPastryService pastryService, IBreadService breadService)
    {/* ... */}

    [Query("pastries/search")]
    public IEnumerable<IPastry> SearchPastries(string nameLike, int maxResults = 50)
    {/* ... */}

    [Query("pastries/recipe")]
    public Recipe RetrieveRecipe(int id)
    {/* ... */}

    [Query("breadCounter/orders")]
    public IEnumerable<BreadOrder> FindOrders(int customerId)
    {/* ... */}
}
```

**This GraphQL Query**

```graphql
query SearchGroceryStore($pastryName: String!) {
  groceryStore {
    bakery {
      pastries {
        search(nameLike: $pastryName) {
          name
          type
        }
        recipe(id: 15) {
          name
          ingredients {
            name
          }
        }
      }
      breadCounter {
        orders(id: 36) {
          id
          items {
            id
            quantity
          }
        }
      }
    }
  }
}
```

#### Add the Package from Nuget\*:

```
> Install-Package GraphQL.AspNet -AllowPrereleaseVersions
```

_\*This library is still in beta_

#### Register GraphQL with your Application:

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // other code and configuration options
    // omitted for brevity
    services.AddGraphQL();
}

public void Configure(IApplicationBuilder appBuilder)
{
    // other code omitted for brevity
    appBuilder.UseGraphQL();
}
```

#### Subscriptions

GraphQL ASP.NET supports web-socket based [subscriptions](https://graphql-aspnet.github.io/docs/advanced/subscriptions) using the Apollo client messaging protocol out of the box. Subscription support can be easily [extended](https://graphql-aspnet.github.io/docs/advanced/subscriptions#scaling-subscription-servers) to multi-server environments and even other messaging protocols.
