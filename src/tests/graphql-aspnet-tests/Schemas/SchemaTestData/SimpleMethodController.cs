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
    using GraphQL.AspNet.Tests.CommonHelpers;

    /// <summary>
    /// A simple controller utilzing all default values with no overrides.
    /// </summary>
    /// <seealso cref="GraphQL.AspNet.Controllers.GraphController" />
    public class SimpleMethodController : GraphController
    {
        [Query]
        public TwoPropertyObject TestActionMethod(string arg1, int arg2)
        {
            return new TwoPropertyObject()
            {
                Property1 = arg1,
                Property2 = arg2,
            };
        }
    }
}