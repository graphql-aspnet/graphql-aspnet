// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.GraphTypeCollectionTestData
{
    using GraphQL.AspNet.Attributes;

    public class ObjectForCollection2
    {
        [GraphField]
        public int Property1 { get; set; }
    }
}