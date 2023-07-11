// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.DefaultSchemaFactoryTestData
{
    using GraphQL.AspNet.Attributes;

    public class CustomObjectWithFieldWithArg
    {
        [GraphField]
        public int MethodWithArg(decimal? arg)
        {
            return 0;
        }
    }
}