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
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("Input Arg: {Name} (GraphType = {GraphType.Name})")]
    internal class DocumentInputArgument : DocumentPartBase, IInputArgumentDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgument" /> class.
        /// </summary>
        /// <param name="parentPart">The document part(a field or directive) that owns this argument.</param>
        /// <param name="node">The node in the query document creating this entity.</param>
        /// <param name="typeExpression">The expected type expression of the argument.</param>
        public DocumentInputArgument(
            IDocumentPart parentPart,
            InputItemNode node,
            GraphTypeExpression typeExpression)
            : base(parentPart, node)
        {
            this.Name = node.InputName.ToString();
            this.TypeExpression = typeExpression;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Argument;

        /// <inheritdoc />
        public ISuppliedValueDocumentPart Value => this
            .Children[DocumentPartType.SuppliedValue]
            .OfType<ISuppliedValueDocumentPart>()
            .FirstOrDefault();
    }
}