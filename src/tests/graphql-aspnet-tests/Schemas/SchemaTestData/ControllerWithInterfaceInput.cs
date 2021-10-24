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

    public class ControllerWithInterfaceInput : GraphController
    {
        [QueryRoot]
        public string DoThing(IPersonData data)
        {
            return string.Empty;
        }
    }
}