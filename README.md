## GraphQL ASP.NET (beta)

### Documentation: [https://graphql-aspnet.github.io](https://graphql-aspnet.github.io)

> Target Framework: **netstandard2.0**

GraphQL ASP.NET is a fully featured graphql library that utilizes a controller/action programming model familiar to ASP.NET MVC developers. Instead of focusing on schemas and mapping resolvers, focus on controllers and models. GraphQL ASP.NET will automatically generate the schema to match your code.

|Recent Builds | |
|--|-|
|Development | [![Develop Branch Build Status](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_apis/build/status/CI%20%26%20Deployment%20Build?branchName=develop)](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_build/latest?definitionId=4&branchName=develop)|
|Master | [![Master Branch Build Status](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_apis/build/status/CI%20%26%20Deployment%20Build?branchName=master)](https://dev.azure.com/graphqlaspnet/GraphQL%20ASP.NET/_build/latest?definitionId=4&branchName=master)|

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
