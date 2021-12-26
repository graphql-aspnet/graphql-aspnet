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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    [DirectiveLocations(ExecutableDirectiveLocation.AllFieldSelections)]
    public class MismatchedSignaturesDirective : GraphDirective
    {
        public Task<IGraphActionResult> BeforeFieldResolution(int arg1, string arg2)
        {
            return null;
        }

        public Task<IGraphActionResult> BeforeFieldResolution(long arg1)
        {
            return null;
        }
    }
}