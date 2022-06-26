﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    internal abstract class DocumentFieldBase : DocumentPartBase<FieldNode>
    {
        protected DocumentFieldBase(
            IDocumentPart parentPart,
            FieldNode node,
            IGraphField field,
            IGraphType fieldGraphType)
            : base(parentPart, node)
        {

            this.AssignGraphType(fieldGraphType);
            this.Field = field;
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
            return new DocumentInputArgumentCollectionDocumentPart(this);
        }

        public virtual IEnumerable<IDirectiveDocumentPart> GatherDirectives()
        {
            return this.Children[DocumentPartType.Directive]
                .OfType<IDirectiveDocumentPart>()
                .ToList();
        }

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
        public IFieldSelectionSetDocumentPart FieldSelectionSet =>
            this.Children[DocumentPartType.FieldSelectionSet]
            .FirstOrDefault() as IFieldSelectionSetDocumentPart;


        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Field;
    }
}