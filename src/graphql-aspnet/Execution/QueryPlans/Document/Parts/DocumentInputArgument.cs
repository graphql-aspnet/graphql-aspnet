// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts
{
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentInputArgument : DocumentPartBase, IInputArgumentDocumentPart
    {
        public DocumentInputArgument(
           IDocumentPart parentPart,
           IGraphArgument argument,
           string inputNanme,
           SourceLocation location)
           : base(parentPart, location)
        {
            this.Name = inputNanme;
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