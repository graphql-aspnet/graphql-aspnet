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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A base class defining common elements for different field types within a query
    /// document.
    /// </summary>
    internal abstract class DocumentFieldBase : DocumentPartBase, IIncludeableDocumentPart
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
            this.Name = node.FieldName;
            this.Alias = node.FieldAlias;

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

        /// <inheritdoc cref="IFieldDocumentPart.PostResolver" />
        public Func<FieldResolutionContext, CancellationToken, Task> PostResolver
        {
            get
            {
                if (this.Attributes.TryGetValue(Constants.DocumentPartAttributes.FieldPostResolutionResolver, out object item))
                    return item as Func<FieldResolutionContext, CancellationToken, Task>;

                return null;
            }

            set
            {
                this.Attributes.TryRemove(Constants.DocumentPartAttributes.FieldPostResolutionResolver, out _);
                this.Attributes.TryAdd(Constants.DocumentPartAttributes.FieldPostResolutionResolver, value);
            }
        }

        /// <inheritdoc cref="IFieldDocumentPart.Field" />
        public IGraphField Field { get; set; }

        /// <inheritdoc cref="IFieldDocumentPart.Name" />
        public ReadOnlyMemory<char> Name { get; }

        /// <inheritdoc cref="IFieldDocumentPart.Alias" />
        public ReadOnlyMemory<char> Alias { get; }

        /// <inheritdoc cref="IFieldDocumentPart.FieldSelectionSet" />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        /// <inheritdoc cref="IFieldDocumentPart.Arguments" />
        public IInputArgumentCollectionDocumentPart Arguments => _arguments;

        /// <inheritdoc cref="IDirectiveContainerDocumentPart.Directives" />
        public IDirectiveCollectionDocumentPart Directives => _directives;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Field;

        /// <inheritdoc cref="ISecureDocumentPart.SecureItem" />
        public ISecureSchemaItem SecureItem => this.Field;

        /// <inheritdoc />
        public bool IsIncluded
        {
            get
            {
                return this.Attributes.ContainsKey(Constants.DocumentPartAttributes.IsIncluded);
            }

            set
            {
                if (value)
                    this.Attributes.TryAdd(Constants.DocumentPartAttributes.IsIncluded, value);
                else
                    this.Attributes.TryRemove(Constants.DocumentPartAttributes.IsIncluded, out _);
            }
        }
    }
}