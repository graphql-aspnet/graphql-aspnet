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
        /// Assigns the document variable this reference references.
        /// </summary>
        /// <param name="variable">The variable.</param>
        void AssignVariable(IVariableDocumentPart variable);

        /// <summary>
        /// Gets the name of the variable this instance references.
        /// </summary>
        /// <value>The name of the variable.</value>
        string VariableName { get; }

        /// <summary>
        /// Gets the declared variable this reference references.
        /// </summary>
        /// <value>The variable.</value>
        IVariableDocumentPart Variable { get; }
    }
}