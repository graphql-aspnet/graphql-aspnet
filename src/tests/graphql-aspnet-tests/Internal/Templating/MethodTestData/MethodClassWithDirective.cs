// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.MethodTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;

    public class MethodClassWithDirective
    {
        [ApplyDirective(typeof(DirectiveWithArgs), 44, "method arg")]
        public int Counter()
        {
            return 0;
        }
    }
}