// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    /// <summary>
    /// contains an overloaded method without providing specific names for each in the
    /// object graph but the methods are on different operation types.
    /// </summary>
    public class TwoMethodsDifferentRootsController : GraphController
    {
        [Query]
        public TwoPropertyObject ActionMethodNoAttributes()
        {
            return new TwoPropertyObject();
        }

        [Mutation]
        public TwoPropertyObject ActionMethodNoAttributes(string input1)
        {
            return new TwoPropertyObject();
        }

        [TypeExtension(typeof(TwoPropertyObject), "Property3", typeof(int))]
        public IGraphActionResult TypeExtensionMethod()
        {
            return null;
        }
    }
}