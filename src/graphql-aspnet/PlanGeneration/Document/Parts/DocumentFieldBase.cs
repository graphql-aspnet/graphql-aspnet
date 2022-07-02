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
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    internal abstract class DocumentFieldBase : DocumentPartBase<FieldNode>
    {
        private DocumentInputArgumentCollection _arguments;

        protected DocumentFieldBase(
            IDocumentPart parentPart,
            FieldNode node,
            IGraphField field,
            IGraphType fieldGraphType)
            : base(parentPart, node)
        {

            this.AssignGraphType(fieldGraphType);
            this.Field = field;

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
            // when there is no target restriction or a direct type match
            if (this.GraphType == null || graphType == this.GraphType)
                return true;

            // also allowed if the provided graphType can masquerade
            // as this target graph type (such as an object type implementing an interface)
            if (graphType is IObjectGraphType obj && obj.InterfaceNames.Contains(this.GraphType.Name))
                return true;

            return false;
        }

        public virtual IInputArgumentCollectionDocumentPart GatherArguments()
        {
            return _arguments;
        }

        public virtual IEnumerable<IDirectiveDocumentPart> GatherDirectives()
        {
            return this.Children[DocumentPartType.Directive]
                .OfType<IDirectiveDocumentPart>()
                .ToList();
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            if (relativeDepth == 1 && childPart is IFieldSelectionSetDocumentPart fss)
                this.FieldSelectionSet = fss;

            if (relativeDepth == 1 && childPart is IInputArgumentDocumentPart iadp)
                _arguments.AddArgumment(iadp);
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

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Field;
    }
}