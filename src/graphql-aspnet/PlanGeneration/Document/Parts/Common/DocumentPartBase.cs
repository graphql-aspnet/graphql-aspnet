// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.Common
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;

    /// <summary>
    /// A base class with common functionality of all <see cref="IDocumentPart" />
    /// implementations.
    /// </summary>
    internal abstract class DocumentPartBase : IDocumentPart
    {
        private SourcePath _path = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBase" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The AST node from which this part was created.</param>
        protected DocumentPartBase(IDocumentPart parentPart, SyntaxNode node)
        {
            this.Attributes = new HashSet<string>();
            this.Parent = Validation.ThrowIfNullOrReturn(parentPart, nameof(Parent));
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));

            this.Children = new DocumentPartsCollection(this);

            // wire up local events
            this.Children.ChildPartAdded += (o, e) => this.OnChildPartAdded(e.TargetDocumentPart, e.RelativeDepth);
        }

        /// <summary>
        /// When overriden in a child field, extends the human readable path into the query document
        /// to include this document part.
        /// </summary>
        /// <param name="path">The path to extend. This path should be cloned.</param>
        /// <returns>SourcePath.</returns>
        protected virtual SourcePath CreatePath(SourcePath path)
        {
            return path.Clone();
        }

        /// <summary>
        /// When overriden in a child class, this method is called
        /// when a child part is added to this instance.
        /// </summary>
        /// <param name="childPart">The child part that was added.</param>
        /// <param name="relativeDepth">The depth of the part relative to this part. A depth of 1 indicates
        /// a direect child, 2 a grand child etc..</param>
        protected virtual void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
        }

        /// <inheritdoc />
        public virtual void AssignGraphType(IGraphType graphType)
        {
            this.GraphType = graphType;
        }

        /// <inheritdoc />
        public abstract DocumentPartType PartType { get; }

        /// <inheritdoc />
        public IDocumentPartsCollection Children { get; }

        /// <inheritdoc />
        public IDocumentPart Parent { get; }

        /// <inheritdoc />
        public IGraphType GraphType { get; private set; }

        /// <inheritdoc />
        public SourcePath Path
        {
            get
            {
                if (_path == null)
                {
                    _path = this.CreatePath(this.Parent.Path);
                }

                return _path;
            }
        }

        /// <inheritdoc />
        public SyntaxNode Node { get; }

        /// <inheritdoc />
        public ISet<string> Attributes { get; }

        /// <inheritdoc />
        public abstract string Description { get; }
    }
}