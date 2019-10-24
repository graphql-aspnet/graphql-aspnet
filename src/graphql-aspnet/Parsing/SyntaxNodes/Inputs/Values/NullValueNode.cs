// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A syntax node representing a parsed "null" or not supplied value.
    /// </summary>
    [DebuggerDisplay("null")]
    public class NullValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullValueNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        public NullValueNode(
            SourceLocation startLocation)
            : base(startLocation, ParserConstants.Keywords.Null)
        {
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return false;
        }
    }
}