﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An instance of a referenced directive in a query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentDirective : DocumentPartBase, IDirectiveDocumentPart, IDescendentDocumentPartSubscriber
    {
        private DocumentInputArgumentCollection _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDirective"/> class.
        /// </summary>
        /// <param name="parentPart">The parent part that owns this directive.</param>
        /// <param name="directiveName">Name of the directive.</param>
        /// <param name="location">The location in the source query where
        /// this directive was declared.</param>
        public DocumentDirective(
            IDocumentPart parentPart,
            ReadOnlySpan<char> directiveName,
            SourceLocation location)
            : base(parentPart, location)
        {
            this.Location = parentPart.AsDirectiveLocation();
            this.DirectiveName = directiveName.ToString();

            _arguments = new DocumentInputArgumentCollection(this);
        }

        /// <inheritdoc />
        protected override SourcePath ExtendPath(SourcePath pathToExtend)
        {
            pathToExtend.AddFieldName(TokenTypeNames.AT_SYMBOL + this.DirectiveName.ToString());
            return pathToExtend;
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart is IInputArgumentDocumentPart iiadp)
                _arguments.AddArgument(iiadp);
        }

        /// <inheritdoc />
        public DirectiveLocation Location { get; }

        /// <inheritdoc />
        public string DirectiveName { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Directive;

        /// <inheritdoc />
        public IInputArgumentCollectionDocumentPart Arguments => _arguments;

        /// <inheritdoc />
        public override string Description => $"Directive {this.DirectiveName ?? "-unknown-"}";

        /// <inheritdoc />
        public ISecurableSchemaItem SecureItem => this.GraphType as IDirective;
    }
}