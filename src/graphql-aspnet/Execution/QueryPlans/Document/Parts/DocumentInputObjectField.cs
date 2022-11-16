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
    internal class DocumentInputObjectField : DocumentPartBase, IInputObjectFieldDocumentPart
    {
        public DocumentInputObjectField(
        IDocumentPart parentPart,
        string suppliedName,
        IInputGraphField field,
        SourceLocation location)
        : base(parentPart, location)
        {
            this.Name = suppliedName;
            this.Field = field;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IInputGraphField Field { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression => this.Field?.TypeExpression;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.InputField;

        /// <inheritdoc />
        public ISuppliedValueDocumentPart Value => this
            .Children[DocumentPartType.SuppliedValue]
            .OfType<ISuppliedValueDocumentPart>()
            .FirstOrDefault();

        /// <inheritdoc />
        public override string Description => $"Input Object Field: {this.Name}";
    }
}