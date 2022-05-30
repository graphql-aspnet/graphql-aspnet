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
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;

    [ApplyDirective(typeof(DirectiveWithArgs), 101, "controller arg")]
    public class ControllerWithDirective : GraphController
    {
    }
}