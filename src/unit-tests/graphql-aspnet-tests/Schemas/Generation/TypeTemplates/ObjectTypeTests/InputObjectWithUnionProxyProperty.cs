// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ObjectTypeTests
{
    using GraphQL.AspNet.Interfaces.Schema;

    public class InputObjectWithUnionProxyProperty
    {
        public int Id { get; set; }

        public IGraphUnionProxy UnionProxy { get; set; }
    }
}