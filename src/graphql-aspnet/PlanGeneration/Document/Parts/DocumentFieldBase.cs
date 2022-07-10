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
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A base class defining common elements for different field types within a query
    /// document.
    /// </summary>
    internal abstract class DocumentFieldBase : DocumentPartBase<FieldNode>, IResolvableDocumentPart
    {
        private readonly DocumentInputArgumentCollection _arguments;
        private readonly DocumentDirectiveCollection _directives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldBase"/> class.
        /// </summary>
        /// <param name="parentPart">The parent part that owns this field.</param>
        /// <param name="node">The node in the AST that defined the creation of this field.</param>
        /// <param name="field">The field referenced from the target schema.</param>
        /// <param name="fieldGraphType">The graph type for data returned from the field.</param>
        protected DocumentFieldBase(
            IDocumentPart parentPart,
            FieldNode node,
            IGraphField field,
            IGraphType fieldGraphType)
            : base(parentPart, node)
        {
            this.AssignGraphType(fieldGraphType);
            this.Field = field;

            _directives = new DocumentDirectiveCollection(this);
            _arguments = new DocumentInputArgumentCollection(this);
            this.IsIncluded = true;
        }

        /// <inheritdoc />
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            thisPath.AddFieldName(this.Name.ToString());
            return thisPath;
        }

        /// <inheritdoc cref="IFieldDocumentPart.CanResolveForGraphType(IGraphType)" />
        public virtual bool CanResolveForGraphType(IGraphType graphType)
        {
            // if the provided graphtype owns this field
            // then yes it can resolve for it
            if (graphType is IGraphFieldContainer fieldContainer)
            {
                if (fieldContainer.Fields.Contains(this.Field))
                    return true;
            }

            // if the target graph type is an object and this field points to
            // an interface that said object implements, its also allowed.
            if (graphType is IObjectGraphType obj)
            {
                if (this.Field.Parent is IInterfaceGraphType igt)
                {
                    if (obj.InterfaceNames.Contains(igt.Name))
                        return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            if (relativeDepth == 1)
            {
                if (childPart is IFieldSelectionSetDocumentPart fss)
                {
                    this.FieldSelectionSet = fss;
                }
                else if (childPart is IInputArgumentDocumentPart iadp)
                {
                    _arguments.AddArgument(iadp);
                }
                else if (childPart is IDirectiveDocumentPart ddp)
                {
                    _directives.AddDirective(ddp);
                }
            }
        }

        /// <inheritdoc cref="IFieldDocumentPart.Field" />
        public IGraphField Field { get; }

        /// <inheritdoc cref="IFieldDocumentPart.Name" />
        public ReadOnlyMemory<char> Name => this.Node.FieldName;

        /// <inheritdoc cref="IFieldDocumentPart.Alias" />
        public ReadOnlyMemory<char> Alias => this.Node.FieldAlias;

        /// <inheritdoc cref="IFieldDocumentPart.FieldSelectionSet" />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        /// <inheritdoc cref="IFieldDocumentPart.Arguments" />
        public IInputArgumentCollectionDocumentPart Arguments => _arguments;

        /// <inheritdoc cref="IDirectiveContainerDocumentPart.Directives" />
        public IDirectiveCollectionDocumentPart Directives => _directives;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Field;

        /// <inheritdoc />
        public bool IsIncluded { get; set; }
    }
}