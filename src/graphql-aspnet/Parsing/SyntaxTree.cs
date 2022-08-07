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
    using System.Diagnostics;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A concrete implemnetation of a the syntax tree to contain operations and fragments
    /// recieved on the text based query.
    /// </summary>
    /// <seealso cref="ISyntaxTree" />
    [DebuggerDisplay("Default Syntax Tree")]
    public class SyntaxTree : ISyntaxTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxTree"/> class.
        /// </summary>
        public SyntaxTree()
        {
            this.RootNode = new DocumentNode();
        }

        /// <inheritdoc />
        public DocumentNode RootNode { get; }
    }
}