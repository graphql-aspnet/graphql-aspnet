// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A base class defining common elements for different field types within a query
    /// document.
    /// </summary>
    internal abstract class DocumentFieldBase : DocumentPartBase, IIncludeableDocumentPart, IDecdendentDocumentPartSubscriber
    {
        private readonly DocumentInputArgumentCollection _arguments;
        private readonly DocumentDirectiveCollection _directives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldBase"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this field.</param>
        /// <param name="fieldName">Name of the field as declared in the source text.</param>
        /// <param name="alias">The alias applied to this field. Value should be set
        /// to the <paramref name="fieldName"/> if no formal alias was supplied.</param>
        /// <param name="field">A reference to the target field, found in the schema.</param>
        /// <param name="fieldGraphType">The graph type returned by this field.</param>
        /// <param name="location">The location in the source text where this field originated.</param>
        protected DocumentFieldBase(
            IDocumentPart parentPart,
            string fieldName,
            string alias,
            IGraphField field,
            IGraphType fieldGraphType,
            SourceLocation location)
            : base(parentPart, location)
        {
            this.Name = fieldName;
            this.Alias = alias;

            this.AssignGraphType(fieldGraphType);
            this.Field = field;

            _directives = new DocumentDirectiveCollection(this);
            _arguments = new DocumentInputArgumentCollection(this);
            this.IsIncluded = true;
        }

        /// <inheritdoc />
        protected override SourcePath ExtendPath(SourcePath pathToExtend)
        {
            pathToExtend.AddFieldName(this.Name);
            return pathToExtend;
        }

        /// <inheritdoc cref="IDecdendentDocumentPartSubscriber.OnDecendentPartAdded" />
        void IDecdendentDocumentPartSubscriber.OnDecendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart.Parent == this)
            {
                if (decendentPart is IFieldSelectionSetDocumentPart fss)
                {
                    this.FieldSelectionSet = fss;
                }
                else if (decendentPart is IInputArgumentDocumentPart iadp)
                {
                    _arguments.AddArgument(iadp);
                }
                else if (decendentPart is IDirectiveDocumentPart ddp)
                {
                    _directives.AddDirective(ddp);
                }
            }
        }

        /// <inheritdoc cref="IFieldDocumentPart.PostResolver" />
        public Func<FieldResolutionContext, CancellationToken, Task> PostResolver { get; set; }

        /// <inheritdoc cref="IFieldDocumentPart.Field" />
        public IGraphField Field { get; set; }

        /// <inheritdoc cref="IFieldDocumentPart.Name" />
        public string Name { get; }

        /// <inheritdoc cref="IFieldDocumentPart.Alias" />
        public string Alias { get; }

        /// <inheritdoc cref="IFieldDocumentPart.FieldSelectionSet" />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        /// <inheritdoc cref="IInputArgumentCollectionContainer.Arguments" />
        public IInputArgumentCollectionDocumentPart Arguments => _arguments;

        /// <inheritdoc cref="IDirectiveContainerDocumentPart.Directives" />
        public IDirectiveCollectionDocumentPart Directives => _directives;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Field;

        /// <inheritdoc cref="ISecurableDocumentPart.SecureItem" />
        public ISecurableSchemaItem SecureItem => this.Field;

        /// <inheritdoc />
        public bool IsIncluded { get; set; }
    }
}