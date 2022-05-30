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
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class TestDirectiveMethodTemplateContainer : GraphDirective
    {
        [GraphSkip]
        public Task<object> SkippedMethod()
        {
            return Task.FromResult(null as object);
        }

        [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
        public Task<object> IncorrrectReturnType(object source)
        {
            return Task.FromResult(null as object);
        }
    }
}