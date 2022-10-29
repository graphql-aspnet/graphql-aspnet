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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A value node that is a container for a collection of other input values.
    /// </summary>
    /// <seealso cref="InputValueNode" />
    [DebuggerDisplay("Count = {Children.Count}")]
    public class ListValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListValueNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        public ListValueNode(SourceLocation startLocation)
            : base(startLocation, ReadOnlyMemory<char>.Empty)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"LV-{this.Value}";
        }
    }
}