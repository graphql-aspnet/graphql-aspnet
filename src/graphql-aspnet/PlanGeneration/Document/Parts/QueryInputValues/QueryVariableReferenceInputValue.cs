// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value that is a pointer to a variable defined in the operation that contains it.
    /// </summary>
    [DebuggerDisplay("Variable Ref: {VariableName}")]
    public class QueryVariableReferenceInputValue : QueryInputValue, IResolvablePointer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVariableReferenceInputValue"/> class.
        /// </summary>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        public QueryVariableReferenceInputValue(VariableValueNode node)
            : base(node)
        {
            this.VariableName = node.Value.ToString();
        }

        /// <summary>
        /// Attaches a variable found within an operation to this input value to carry for future operations.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void AssignVariableReference(QueryVariable variable)
        {
            this.Variable = Validation.ThrowIfNullOrReturn(variable, nameof(variable));
        }

        /// <summary>
        /// Gets the name of the variable this instance references.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

        /// <summary>
        /// Gets a reference to the variable instance in the operation that this value points to.
        /// </summary>
        /// <value>The variable.</value>
        public QueryVariable Variable { get; private set; }

        /// <summary>
        /// Gets the name of the item pointed to by this instance.
        /// </summary>
        /// <value>The points to.</value>
        string IResolvablePointer.PointsTo => this.Variable.Name;

        /// <summary>
        /// Gets the fallback resolvable item should the item this pointer points to not exist.
        /// </summary>
        /// <value>The default item.</value>
        IResolvableItem IResolvablePointer.DefaultItem => this.Variable.Value;
    }
}