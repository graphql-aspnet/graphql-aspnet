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

    public class ObjectWithMethodOverloads
    {
        [GraphField]
        public int Method1()
        {
            return 0;
        }

        public int Method1(string arg1)
        {
            return 0;
        }
    }
}