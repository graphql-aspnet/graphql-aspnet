// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    public static class SyntaxNodeExtensions
    {
        public static TNode FirstOrDefault<TNode>(this IEnumerable<SyntaxNode> nodeList)
            where TNode : SyntaxNode
        {
            return nodeList.FirstOrDefault(x => x is TNode) as TNode;
        }
    }
}