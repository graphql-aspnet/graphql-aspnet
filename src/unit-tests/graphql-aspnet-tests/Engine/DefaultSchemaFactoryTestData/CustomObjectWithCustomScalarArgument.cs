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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class CustomObjectWithCustomScalarArgument
    {
        [GraphField]
        public int FieldWithScalarArg(TwoPropertyObject obj)
        {
            return 0;
        }
    }
}