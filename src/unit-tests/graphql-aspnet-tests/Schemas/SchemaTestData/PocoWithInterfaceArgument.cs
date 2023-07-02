// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.Interfaces;

    public class PocoWithInterfaceArgument
    {
        [GraphField]
        public string DoMethod(ISinglePropertyObject obj)
        {
            return string.Empty;
        }
    }
}