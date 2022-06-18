// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;

    /// <summary>
    /// A supplied value in a query document representing a variable defined on the
    /// query operation that owns this document part.
    /// </summary>
    public interface IVariableReferenceDocumentPart : ISuppliedValueDocumentPart, IResolvablePointer
    {
        /// <summary>
        /// Attaches the formal variable declaration found within an operation
        /// to this input value to carry for future dereferencing.
        /// </summary>
        /// <param name="variable">The variable to attach.</param>
        internal void AssignVariableReference(IQueryVariableDocumentPart variable);

        /// <summary>
        /// Gets the name of the variable this instance references.
        /// </summary>
        /// <value>The name of the variable.</value>
        string VariableName { get; }

        /// <summary>
        /// Gets a reference to the variable instance in the operation that this value points to.
        /// </summary>
        /// <value>The variable.</value>
        IQueryVariableDocumentPart Variable { get;  }
    }
}