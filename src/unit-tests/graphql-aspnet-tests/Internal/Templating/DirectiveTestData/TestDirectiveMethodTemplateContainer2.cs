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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class TestDirectiveMethodTemplateContainer2 : GraphDirective
    {
        public Task<IGraphActionResult> NotAValidMethodName()
        {
            return Task.FromResult(null as IGraphActionResult);
        }

        [DirectiveLocations(DirectiveLocation.FIELD)]
        public Task InvalidTaskReference(object source)
        {
            return null;
        }

        [DirectiveLocations(DirectiveLocation.SCALAR)]
        public IGraphActionResult NoSourceItemDeclaration()
        {
            return null;
        }

        [DirectiveLocations(DirectiveLocation.QUERY)]
        public IGraphActionResult InterfaceAsParameter(ITwoPropertyObject obj)
        {
            return null;
        }
    }
}