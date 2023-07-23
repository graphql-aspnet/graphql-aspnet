// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ControllerWithInternalNames : GraphController
    {
        [QueryRoot(InternalName = "ActionWithInternalName")]
        public TwoPropertyObject ActionField()
        {
            return null;
        }

        [TypeExtension(typeof(TwoPropertyObject), "field1", InternalName = "TypeExtensionInternalName")]
        public int TypeExpressionField()
        {
            return 0;
        }
    }
}