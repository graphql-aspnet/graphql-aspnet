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

    internal abstract class DocumentFieldBase : DocumentPartBase<FieldNode>
    {
        private readonly DocumentInputArgumentCollection _arguments;
        private readonly DocumentDirectiveCollection _directives;

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
        }

        /// <inheritdoc />
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            thisPath.AddFieldName(this.Name.ToString());
            return thisPath;
        }

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

        /// <summary>
        /// Gets a reference to the field, within the schema, this document part points to.
        /// </summary>
        /// <value>The schema field refrence.</value>
        public IGraphField Field { get; }

        /// <summary>
        /// Gets the name of the field declared by this part.
        /// </summary>
        /// <value>The name.</value>
        public ReadOnlyMemory<char> Name => this.Node.FieldName;

        /// <summary>
        /// Gets the alias to apply to the field declared by this part.
        /// </summary>
        /// <value>The alias.</value>
        public ReadOnlyMemory<char> Alias => this.Node.FieldAlias;

        /// <summary>
        /// Gets the set of fields to query off a resolved value from this field.
        /// </summary>
        /// <value>The field selection set.</value>
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        public IInputArgumentCollectionDocumentPart Arguments => _arguments;

        public IDirectiveCollectionDocumentPart Directives => _directives;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Field;
    }
}