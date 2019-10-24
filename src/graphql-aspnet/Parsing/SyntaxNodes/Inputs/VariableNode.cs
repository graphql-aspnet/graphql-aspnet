// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A represntion of an as-yet-to-be-defined input value.
    /// </summary>
    [DebuggerDisplay("${Name} (Type: {TypeExpression})")]
    public class VariableNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location of the variable declaration.</param>
        /// <param name="name">The name assigned to the variable, including the '$'.</param>
        /// <param name="typeExpression">The type expression declared for the variable.</param>
        public VariableNode(
            SourceLocation startLocation,
            ReadOnlyMemory<char> name,
            ReadOnlyMemory<char> typeExpression)
            : base(startLocation)
        {
            this.Name = name;
            this.TypeExpression = typeExpression;
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is InputValueNode;
        }

        /// <summary>
        /// Gets the name of the variable as defined in the query.
        /// </summary>
        /// <value>The name of the variable.</value>
        public ReadOnlyMemory<char> Name { get; }

        /// <summary>
        /// Gets the full graph type declaration of the variable as defined in the query in the syntax definition
        /// language. (e.g.  '[SomeType!]!' ). Does not garuntee that the syntax is valid.
        /// </summary>
        /// <value>The type of the variable.</value>
        public ReadOnlyMemory<char> TypeExpression { get; }
    }
}