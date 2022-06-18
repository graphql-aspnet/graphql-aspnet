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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value that is a pointer to a variable defined in the operation that contains it.
    /// </summary>
    [DebuggerDisplay("Variable Ref: {VariableName}")]
    public class DocumentVariableReferenceInputValue : DocumentSuppliedValue, IVariableReferenceDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariableReferenceInputValue"/> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        public DocumentVariableReferenceInputValue(VariableValueNode node)
            : base(node)
        {
            this.VariableName = node.Value.ToString();
        }

        /// <inheritdoc />
        public void AssignVariableReference(IQueryVariableDocumentPart variable)
        {
            this.Variable = Validation.ThrowIfNullOrReturn(variable, nameof(variable));
        }

        /// <inheritdoc />
        public string VariableName { get; }

        /// <inheritdoc />
        public IQueryVariableDocumentPart Variable { get; private set; }

        /// <inheritdoc />
        public string PointsTo => this.Variable.Name;

        /// <inheritdoc />
        public IResolvableItem DefaultItem => this.Variable.Value;
    }
}