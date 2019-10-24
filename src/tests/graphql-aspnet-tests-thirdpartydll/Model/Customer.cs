// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ThirdPartyDll.Model
{
    using GraphQL.AspNet.Attributes;

    public class Customer
    {
        [GraphField]
        public int Id { get; set; }

        [GraphField]
        public string Name { get; set; }
    }
}