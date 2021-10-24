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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    /// <summary>
    /// contains an overloaded method but DOES provide specific names to create a
    /// unique route per method.
    /// </summary>
    public class TwoMethodNoClashController : GraphController
    {
        [Mutation]
        public TwoPropertyObject ActionMethodNoAttributes()
        {
            return new TwoPropertyObject();
        }

        [Mutation("Action2")]
        public TwoPropertyObject ActionMethodNoAttributes(string input1)
        {
            return new TwoPropertyObject();
        }
    }
}