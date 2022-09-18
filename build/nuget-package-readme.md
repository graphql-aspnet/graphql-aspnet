## GraphQL ASP.NET

### Documentation: [https://graphql-aspnet.github.io](https://graphql-aspnet.github.io)

GraphQL ASP.NET is a fully featured graphql library that utilizes a controller/action programming model similar to ASP.NET.

**This Controller**

```csharp
// BakeryController.cs
[GraphRoute("groceryStore/bakery")]
public class BakeryController : GraphController
{
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
