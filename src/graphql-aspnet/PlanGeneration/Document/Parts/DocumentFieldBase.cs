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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A base class defining common elements for different field types within a query
    /// document.
    /// </summary>
    internal abstract class DocumentFieldBase : DocumentPartBase, IIncludeableDocumentPart, IDecdendentDocumentPartSubscriber
    {
        private readonly DocumentInputArgumentCollection _arguments;
        private readonly DocumentDirectiveCollection _directives;

        protected DocumentFieldBase(
            IDocumentPart parentPart,
            string fieldName,
            IGraphField field,
            IGraphType fieldGraphType,
            SourceLocation location,
            string alias)
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

        /// <inheritdoc cref="ISecureDocumentPart.SecureItem" />
        public ISecureSchemaItem SecureItem => this.Field;

        /// <inheritdoc />
        public bool IsIncluded { get; set; }
    }
}