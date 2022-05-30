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
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class MismatchedSignaturesDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD)]
        public Task<IGraphActionResult> Execute1(int arg1, string arg2)
        {
            return null;
        }

        [DirectiveLocations(DirectiveLocation.INLINE_FRAGMENT)]
        public Task<IGraphActionResult> Execute2(long arg1)
        {
            return null;
        }
    }
}