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
    /// An incoming enum value supplied by the client.
    /// </summary>
    /// <seealso cref="InputValueNode" />
    [DebuggerDisplay("Enum: {Value}")]
    public class EnumValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValueNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="value">The value.</param>
        public EnumValueNode(
            SourceLocation startLocation,
            ReadOnlyMemory<char> value)
            : base(startLocation,  value)
        {
        }
    }
}