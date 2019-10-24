## GraphQL ASP.NET (beta)

### Documentation: [https://graphql-aspnet.github.io](https://graphql-aspnet.github.io)

GraphQL ASP.NET is a fully featured graphql library that utilizes a controller/action programming model familiar to ASP.NET MVC developers. Instead of focusing on schemas and mapping resolvers, focus on controllers and models. GraphQL ASP.NET will automatically generate the schema to match your code.

**This Controller**

```csharp
// BakeryController.cs
[GraphRoute("groceryStore/bakery")]
public class BakeryController : GraphController
{
    // Automatic "scoped" dependency injection
    public BakerController(IPastryService pastryService, IBreadService breadService)
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
