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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A single field of data requested on a user's query document.
    /// </summary>
    [DebuggerDisplay("Field: {Field.Name} (Returns: {GraphType.Name}, Restricted: {TargetGraphType.Name)")]
    internal class DocumentFieldSelection : DocumentPartBase<IFieldSelectionSetDocumentPart>, IFieldSelectionDocumentPart
    {
        private readonly List<IDirectiveDocumentPart> _rankedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelection" /> class.
        /// </summary>
        /// <param name="node">The node representing the field in the query document.</param>
        /// <param name="field">The field as its defined in the target schema.</param>
        /// <param name="fieldGraphType">The qualified graph type returned by the field.</param>
        public DocumentFieldSelection(FieldNode node, IGraphField field, IGraphType fieldGraphType)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(fieldGraphType, nameof(fieldGraphType));
            this.Field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Arguments = new DocumentInputArgumentCollection(this);
            this.UpdatePath(null);

            _rankedDirectives = new List<IDirectiveDocumentPart>();
        }

        /// <inheritdoc />
        public void UpdatePath(SourcePath parentPath)
        {
            if (parentPath != null)
                this.Path = parentPath.Clone();
            else
                this.Path = new SourcePath();

            this.Path.AddFieldName(this.Field.Name);
        }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart CreateFieldSelectionSet()
        {
            if (this.FieldSelectionSet == null)
            {
                this.FieldSelectionSet = new DocumentFieldSelectionSet(this.GraphType, this.Path, this);
            }

            return this.FieldSelectionSet;
        }

        /// <inheritdoc />
        public void InsertDirective(IDirectiveDocumentPart directive)
        {
            Validation.ThrowIfNull(directive, nameof(directive));
            _rankedDirectives.Add(directive);
            directive.AssignParent(this);
        }

        /// <inheritdoc />
        public bool CanResolveForGraphType(IGraphType graphType)
        {
            // when there is no target restriction or a direct type match
            if (this.TargetGraphType == null || graphType == this.TargetGraphType)
                return true;

            // also allowed if the provided graphType can masquerade
            // as this target graph type (such as an object type implementing an interface)
            if (graphType is IObjectGraphType obj && obj.InterfaceNames.Contains(this.TargetGraphType.Name))
                return true;

            return false;
        }

        /// <inheritdoc />
        public void AddArgument(IInputArgumentDocumentPart argument)
        {
            this.Arguments.AddArgument(argument);
        }

        /// <inheritdoc />
        public FieldNode Node { get; }

        /// <inheritdoc />
        public IGraphType TargetGraphType { get; set; }

        /// <inheritdoc />
        public IGraphType GraphType { get; }

        /// <inheritdoc />
        public IGraphField Field { get; }

        /// <inheritdoc />
        public IEnumerable<IDirectiveDocumentPart> Directives => _rankedDirectives;

        /// <inheritdoc />
        public IInputArgumentCollectionDocumentPart Arguments { get; }

        /// <inheritdoc />
        public SourcePath Path { get; private set; }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        /// <inheritdoc />
        public ReadOnlyMemory<char> Name => this.Node.FieldName;

        /// <inheritdoc />
        public ReadOnlyMemory<char> Alias => this.Node.FieldAlias;

        /// <inheritdoc />
        public override IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var directive in this.Directives)
                    yield return directive;

                foreach (var argument in this.Arguments.Values)
                    yield return argument;

                if (this.FieldSelectionSet != null)
                    yield return this.FieldSelectionSet;
            }
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FieldSelection;
    }
}