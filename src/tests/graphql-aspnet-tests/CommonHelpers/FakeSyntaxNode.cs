// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    public class FakeSyntaxNode : SyntaxNode
    {
        public FakeSyntaxNode()
            : base(SourceLocation.None)
        {
        }

        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return false;
        }
    }
}