// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using GraphQL.AspNet.Parsing2.Lexing;
    using GraphQL.AspNet.Parsing2;

    /// <summary>
    /// A builder object that will construct a single node of a <see cref="SyntaxTree"/>
    /// from a stream of lexed token values.
    /// </summary>
    public interface ISyntaxNodeBuilder
    {
        /// <summary>
        /// Reads the <paramref name="tokenStream" /> and creates a node of a specific type
        /// and appends it, and any expected children, directly to the provided <paramref name="synTree" />.
        /// When this method completes, the token stream will be pointing at the token
        /// just beyond the end of the node this builder creates.
        /// </summary>
        /// <param name="synTree">The syntax tree where the node will be added.</param>
        /// <param name="parentNode">The parent node that this builder will
        /// add its new child node to.</param>
        /// <param name="tokenStream">The token stream to read from.</param>
        void BuildNode(ref SyntaxTree synTree, ref SyntaxNode parentNode, ref TokenStream tokenStream);
    }
}