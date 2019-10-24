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

    public class TestDirectiveMethodTemplateContainer : GraphDirective
    {
        [GraphSkip]
        public Task<object> SkippedMethod()
        {
            return Task.FromResult(null as object);
        }

        public Task<object> AfterFieldResolution()
        {
            return Task.FromResult(null as object);
        }

        public Task<IGraphActionResult> NotADirectiveMethod()
        {
            return Task.FromResult(null as IGraphActionResult);
        }

        public Task<IGraphActionResult> BeforeFieldResolution()
        {
            return Task.FromResult(null as IGraphActionResult);
        }
    }
}