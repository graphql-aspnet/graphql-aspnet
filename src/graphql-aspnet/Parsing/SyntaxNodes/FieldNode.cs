﻿// *************************************************************
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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A node reprsenting a single field of data requested by a graph operation invocation.
    /// </summary>
    [DebuggerDisplay("Field: {FieldName} (Alias: {FieldAlias})")]
    public class FieldNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        public FieldNode(SourceLocation startLocation, ReadOnlyMemory<char> alias, ReadOnlyMemory<char> name)
            : base(startLocation)
        {
            this.FieldAlias = alias;
            this.FieldName = name;
        }

        /// <summary>
        /// Gets the defined name of the field.
        /// </summary>
        /// <value>The name of the field.</value>
        public ReadOnlyMemory<char> FieldName { get; }

        /// <summary>
        /// Gets the alias set to the field on the outbound projection.
        /// </summary>
        /// <value>The field alias.</value>
        public ReadOnlyMemory<char> FieldAlias { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"F-{this.FieldName}";
        }
    }
}