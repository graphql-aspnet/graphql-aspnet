// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentInputArgument : DocumentPartBase, IInputArgumentDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputArgument" /> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="argument">The formal argument definition found in the target schema.</param>
        /// <param name="inputName">The name of hte input argument as it was declared in the source
        /// text.</param>
        /// <param name="location">The location in the source text where this
        /// document part originated.</param>
        public DocumentInputArgument(
           IDocumentPart parentPart,
           IGraphArgument argument,
           string inputName,
           SourceLocation location)
           : base(parentPart, location)
        {
            this.Name = inputName;
            this.Argument = argument;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IGraphArgument Argument { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression => this.Argument?.TypeExpression;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Argument;

        /// <inheritdoc />
        public ISuppliedValueDocumentPart Value => this
            .Children[DocumentPartType.SuppliedValue]
            .OfType<ISuppliedValueDocumentPart>()
            .FirstOrDefault();

        /// <inheritdoc />
        public override string Description
        {
            get
            {
                if (this.Parent is IFieldDocumentPart)
                    return $"Field Argument: {this.Name}";
                else if (this.Parent is IComplexSuppliedValueDocumentPart)
                    return $"Input Object Field: {this.Name}";
                else
                    return $"Argument: {this.Name}";
            }
        }
    }
}