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
    /// An input value that is a pointer to a supplied variable and not a
    ///  static value.
    /// </summary>
    /// <seealso cref="InputValueNode" />
    [DebuggerDisplay("Variable Ref: ${Value}")]
    public class VariableValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableValueNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="variableName">Name of the variable.</param>
        public VariableValueNode(
            SourceLocation startLocation,
            ReadOnlyMemory<char> variableName)
            : base(startLocation, variableName)
        {
        }
    }
}