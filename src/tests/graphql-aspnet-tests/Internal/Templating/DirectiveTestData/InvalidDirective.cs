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
    using GraphQL.AspNet.Directives;

    public class InvalidDirective : GraphDirective
    {
        public Task<int> BeforeFieldResolution(int arg1, string arg2)
        {
            return null;
        }
    }
}