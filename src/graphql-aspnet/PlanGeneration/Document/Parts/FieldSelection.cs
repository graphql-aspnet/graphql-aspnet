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
    public class FieldSelection : IFieldContainerDocumentPart, IDirectiveContainerDocumentPart, IQueryArgumentContainerDocumentPart, ITargetedDocumentPart, IDocumentPart
    {
        private readonly List<(int Rank, QueryDirective Directive)> _rankedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelection" /> class.
        /// </summary>
        /// <param name="node">The node representing the field in the query document.</param>
        /// <param name="field">The field as its defined in the target schema.</param>
        /// <param name="fieldGraphType">The qualified graph type returned by the field.</param>
        public FieldSelection(FieldNode node, IGraphField field, IGraphType fieldGraphType)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(fieldGraphType, nameof(fieldGraphType));
            this.Field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            _rankedDirectives = new List<(int Rank, QueryDirective Directive)>();
            this.Arguments = new QueryInputArgumentCollection();
            this.UpdatePath(null);
        }

        /// <summary>
        /// Sets the path of this field to be nested under the supplied parent.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        public void UpdatePath(SourcePath parentPath)
        {
            if (parentPath != null)
                this.Path = parentPath.Clone();
            else
                this.Path = new SourcePath();

            this.Path.AddFieldName(this.Field.Name);
        }

        /// <summary>
        /// Creates this field's child selection set. Prior to calling this method this instance
        /// has no possible children.
        /// </summary>
        /// <returns>FieldSelectionSet.</returns>
        public FieldSelectionSet CreateFieldSelectionSet()
        {
            if (this.FieldSelectionSet == null)
            {
                this.FieldSelectionSet = new FieldSelectionSet(this.GraphType, this.Path);
            }

            return this.FieldSelectionSet;
        }

        /// <summary>
        /// Inserts the directive into this document part at the head of its directive set.
        /// </summary>
        /// <param name="directive">The directive to add to this instance.</param>
        /// <param name="rank">The relative rank of this directive to others this instance might container.
        /// Directives are executed in ascending order by the engine.</param>
        public void InsertDirective(QueryDirective directive, int rank)
        {
            _rankedDirectives.Add((rank, directive));
        }

        /// <summary>
        /// Determines whether this field was flagged in the user's query document to be returned by the given graph type.
        /// </summary>
        /// <param name="graphType">Type of the source graph.</param>
        /// <returns><c>true</c> if this field should be returned the specified graph type; otherwise, <c>false</c>.</returns>
        public bool ShouldResolveForGraphType(IGraphType graphType)
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

        /// <summary>
        /// Adds the argument to the collection of arguments on this instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void AddArgument(QueryInputArgument argument)
        {
            this.Arguments.AddArgument(argument);
        }

        /// <summary>
        /// Gets the node that generated this instance.
        /// </summary>
        /// <value>The node.</value>
        public FieldNode Node { get; }

        /// <summary>
        /// Gets or sets the specific target type of the field, if any. Usually set as a result of
        /// spreading a fragment into a selection set.
        /// </summary>
        /// <value>A single graph type to restrict this field to. May be null.</value>
        public IGraphType TargetGraphType { get; set; }

        /// <summary>
        /// Gets the graph type returned by this field as indicated by the schema.
        /// </summary>
        /// <value>The graph type of this field.</value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the field reference pointed to by this instance as its declared in the schema.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field { get; }

        /// <summary>
        /// Gets the directives assigned to this instance.
        /// </summary>
        /// <value>The directives.</value>
        public IEnumerable<QueryDirective> Directives => _rankedDirectives
            .OrderBy(x => x.Rank)
            .Select(x => x.Directive);

        /// <summary>
        /// Gets a collection of input arguments arguments that have been declared in the query document that should be
        /// applied to this field.
        /// </summary>
        /// <value>The arguments.</value>
        public IQueryInputArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets the into the document where this selection set exists.
        /// </summary>
        /// <value>The root path.</value>
        public SourcePath Path { get; private set; }

        /// <summary>
        /// Gets the child field selection set defined in the query document, if any.  May be null if no child fields have
        /// been requested or if this is a leaf field.
        /// </summary>
        /// <value>The field selection set.</value>
        public FieldSelectionSet FieldSelectionSet { get; private set; }

        /// <summary>
        /// Gets the name of the field requested, as it exists in the schema.
        /// </summary>
        /// <value>The parsed name from the queryt document.</value>
        public ReadOnlyMemory<char> Name => this.Node.FieldName;

        /// <summary>
        /// Gets the alias assigned to the field requested as it was defined in the user's query document.
        /// Defaults to <see cref="Name"/> if not supplied.
        /// </summary>
        /// <value>The parsed alias from the query document.</value>
        public ReadOnlyMemory<char> Alias => this.Node.FieldAlias;

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children
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
    }
}