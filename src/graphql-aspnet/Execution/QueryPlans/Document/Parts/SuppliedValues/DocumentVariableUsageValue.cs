// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// An input value that is a pointer to a variable defined in the operation that contains it.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentVariableUsageValue : DocumentSuppliedValue, IVariableUsageDocumentPart
    {
        public DocumentVariableUsageValue(IDocumentPart parentPart, string variableName, SourceLocation location, string key = null)
            : base(parentPart, location, key)
        {
            this.VariableName = variableName;
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IVariableUsageDocumentPart))
                return false;

            var otherVar = value as IVariableUsageDocumentPart;
            return this.VariableName == otherVar.VariableName;
        }

        /// <inheritdoc />
        public string VariableName { get; }

        /// <inheritdoc />
        public string PointsTo => this.VariableName.ToString();

        /// <inheritdoc />
        public override string Description => $"Variable Usage: {this.VariableName}";
    }
}