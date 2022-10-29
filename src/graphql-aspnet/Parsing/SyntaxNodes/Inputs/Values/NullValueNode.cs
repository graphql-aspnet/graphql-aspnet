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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"NV-{this.Value}";
        }
    }
}