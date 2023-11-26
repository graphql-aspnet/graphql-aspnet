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
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class CustomObjectWithCustomScalarField
    {
        [GraphField]
        public TwoPropertyObject FieldWithScalarReturnValue(int arg)
        {
            return null;
        }
    }
}