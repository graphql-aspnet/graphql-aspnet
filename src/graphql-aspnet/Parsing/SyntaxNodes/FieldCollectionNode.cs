// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// Represents a group of field nodes.
    /// </summary>
    /// <seealso cref="SyntaxNode" />
    [DebuggerDisplay("Field Collection (Count = {Children.Count})")]
    public class FieldCollectionNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldCollectionNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        public FieldCollectionNode(SourceLocation startLocation)
            : base(startLocation)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "FC";
        }
    }
}