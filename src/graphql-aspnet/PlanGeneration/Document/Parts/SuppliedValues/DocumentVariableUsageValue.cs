// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value that is a pointer to a variable defined in the operation that contains it.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentVariableUsageValue : DocumentSuppliedValue, IVariableUsageDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariableUsageValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentVariableUsageValue(IDocumentPart parentPart, VariableValueNode node, string key = null)
            : base(parentPart, node, key)
        {
            this.VariableName = node.Value;
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IVariableUsageDocumentPart))
                return false;

            var otherVar = value as IVariableUsageDocumentPart;
            return this.VariableName.Span.SequenceEqual(otherVar.VariableName.Span);
        }

        /// <inheritdoc />
        public ReadOnlyMemory<char> VariableName { get; }

        /// <inheritdoc />
        public string PointsTo => this.VariableName.ToString();

        /// <inheritdoc />
        public override string Description => $"Variable Usage: {VariableName}";
    }
}