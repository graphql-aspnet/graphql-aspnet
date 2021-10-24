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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ControllerWithDirectAndIndirectTypeExtension : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject DoThing(TwoPropertyObject data)
        {
            return data;
        }

        [QueryRoot]
        public ITwoPropertyObject DoOtherThing(int number)
        {
            return null;
        }

        [TypeExtension(typeof(ITwoPropertyObject), "Property3")]
        public string ExtendThroughInterface(ITwoPropertyObject obj)
        {
            return string.Empty;
        }

        [TypeExtension(typeof(TwoPropertyObject), "Property3")]
        public string ExtendThroughObject(TwoPropertyObject obj)
        {
            return string.Empty;
        }
    }
}