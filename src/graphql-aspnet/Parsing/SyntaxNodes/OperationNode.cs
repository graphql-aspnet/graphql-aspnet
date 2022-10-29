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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// A top level node identifying the operation type being described. (e.g. Mutation, Query, Exception).
    /// </summary>
    [DebuggerDisplay("{OperationType}, Name = {OperationName}")]
    public class OperationNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="name">The name.</param>
        public OperationNode(SourceLocation startLocation, ReadOnlyMemory<char> operationType, ReadOnlyMemory<char> name)
            : base(startLocation)
        {
            this.OperationType = operationType;
            this.OperationName = name;
        }

        /// <summary>
        /// Gets the root being targeted by this operation.
        /// </summary>
        /// <value>The root.</value>
        public ReadOnlyMemory<char> OperationType { get; }

        /// <summary>
        /// Gets the name of the operation.
        /// </summary>
        /// <value>The name of the operation.</value>
        public ReadOnlyMemory<char> OperationName { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"O-{this.OperationName.ToString()}";
        }
    }
}