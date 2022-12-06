// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ThirdPartyDll
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.ThirdPartyDll.Model;

    [GraphRoute("customers")]
    public class CustomerController : GraphController
    {
        [Query("customer")]
        public Customer RetrieveCustomer(int customerId)
        {
            return null;
        }
    }
}