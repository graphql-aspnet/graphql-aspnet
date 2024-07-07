## GraphQL ASP.NET

### Documentation: [https://graphql-aspnet.github.io](https://graphql-aspnet.github.io)

> Targets:  **netstandard2.0, net6.0, net8.0**

[![CI-CD](https://github.com/graphql-aspnet/graphql-aspnet/actions/workflows/ci-build.yml/badge.svg?branch=master)](https://github.com/graphql-aspnet/graphql-aspnet/actions/workflows/ci-build.yml) 


GraphQL ASP.NET is a fully featured graphql library that utilizes a controller/action programming model familiar to ASP.NET developers. Instead of focusing on schemas and resolvers, the focus on controllers and model objects. This library will automatically generate the schema to match your code.

✅ Controller-Based Programming Model similar to ASP.NET
<br />
✅ No Boilerplate Code


✏️ **Write This Controller**

```csharp
// BakeryController.cs
[GraphRoute("groceryStore/bakery")]
public class BakeryController : GraphController
{
    [Query("pastries/search")]
    public IEnumerable<IPastry> SearchPastries(string nameLike)
    {/* ... */}

    [Query("pastries/recipe")]
    public Recipe RetrieveRecipe(int id)
    {/* ... */}

    [Query("breadCounter/orders")]
    public IEnumerable<BreadOrder> FindOrders(int customerId)
    {/* ... */}
}
```

▶️ **Execute This Query**

```graphql
query {
  groceryStore {
    bakery {
      pastries {
        search(nameLike: "donut") {
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

#### 📦 Add the Package from Nuget:

```powershell
# Package Manager Console
> Install-Package GraphQL.AspNet

# cli
> dotnet add package GraphQL.AspNet
```


#### 📐 Register GraphQL with your Application:

```csharp
// Program.cs 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGraphQL();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGraphQL();
app.Run();
```

#### Subscriptions

GraphQL ASP.NET supports web-socket based [subscriptions](https://graphql-aspnet.github.io/docs/advanced/subscriptions) out of the box. Subscription support can be [extended](https://graphql-aspnet.github.io/docs/advanced/subscriptions#scaling-subscription-servers) to multi-server environments and even other messaging protocols.
