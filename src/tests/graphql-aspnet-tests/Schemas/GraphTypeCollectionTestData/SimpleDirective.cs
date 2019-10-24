// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.GraphTypeCollectionTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class SimpleDirective : GraphDirective
    {
        public Task<IGraphActionResult> BeforeFieldResolution()
        {
            return null;
        }
    }
}