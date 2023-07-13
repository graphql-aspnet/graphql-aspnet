// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.InputValueNodeTestData
{
    using GraphQL.AspNet.Attributes;

    public class CoffeeCan
    {
        [GraphField]
        public int Id { get; set; }

        [GraphField]
        public string Brand { get; set; }
    }
}