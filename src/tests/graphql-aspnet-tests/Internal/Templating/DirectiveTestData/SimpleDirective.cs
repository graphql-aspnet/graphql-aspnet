// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    [Description("Simple Description")]
    [DirectiveLocations(ExecutableDirectiveLocation.FIELD)]
    public class SimpleDirective : GraphDirective
    {
        public Task<IGraphActionResult> BeforeFieldResolution(int arg1, string arg2)
        {
            return Task.FromResult(null as IGraphActionResult);
        }
    }
}