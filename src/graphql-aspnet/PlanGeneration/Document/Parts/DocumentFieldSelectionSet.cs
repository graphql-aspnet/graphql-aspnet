// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A collection of fields (from a <see cref="IGraphType"/>) that are requested by a user and defined
    /// on their query document. Selected fields are keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("FIELD SET: Graph Type: {GraphType.Name}")]
    internal class DocumentFieldSelectionSet : DocumentPartBase, IFieldSelectionSetDocumentPart, IDocumentPart
    {
        private ExecutionFieldSet _executionSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelectionSet" /> class.
        /// </summary>
        /// <param name="parent">The parent document part that owns this set.</param>
        public DocumentFieldSelectionSet(IDocumentPart parent)
            : base(parent, EmptyNode.Instance)
        {
            this.AssignGraphType(parent.GraphType);
            _executionSet = new ExecutionFieldSet(this);
        }

        /// <inheritdoc />
        public IReadOnlyList<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias)
        {
            return this.ExecutableFields.FilterByAlias(alias);
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FieldSelectionSet;

        /// <inheritdoc />
        public IExecutableFieldSelectionSet ExecutableFields => _executionSet;
    }
}