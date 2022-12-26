// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    using System.Collections.Generic;

    /// <summary>
    /// A representation of a collection of variables on a given operation defined in a user's query document.
    /// </summary>
    public interface IVariableCollectionDocumentPart : IEnumerable<IVariableDocumentPart>
    {
        /// <summary>
        /// Determines whether the specified variable key represents a variable that was duplicated
        /// in the operation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is duplicate; otherwise, <c>false</c>.</returns>
        bool IsDuplicated(string key);

        /// <summary>
        /// Declares that the variable with the supplied name has been referenced and used
        /// within the <see cref="Operation"/> at least once.
        /// </summary>
        /// <param name="variableName">Name of the variable to mark as referenced.</param>
        void MarkAsReferenced(string variableName);

        /// <summary>
        /// Determines whether the specified variable name has been referenced at least once
        /// in the parent <see cref="Operation"/>.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns><c>true</c> if the specified variable name is referenced; otherwise, <c>false</c>.</returns>
        bool IsReferenced(string variableName);

        /// <summary>
        /// Clears all reference marks, resetting any contained variables to a "non referenced"
        /// state.
        /// </summary>
        void ClearReferences();

        /// <summary>
        /// Determines whether this instance contains a variable with the given name.
        /// </summary>
        /// <param name="variableName">The key.</param>
        /// <returns><c>true</c> if the variable exists in this collection; otherwise, <c>false</c>.</returns>
        bool Contains(string variableName);

        /// <summary>
        /// Attempts to retrieve a variable with the given name from this collection.
        /// </summary>
        /// <param name="variableName">The name of the variable to found.</param>
        /// <param name="variable">If a variable is found, it is assigned to this parameter.</param>
        /// <returns><c>true</c> if a variable was found, <c>false</c> otherwise.</returns>
        bool TryGetValue(string variableName, out IVariableDocumentPart variable);

        /// <summary>
        /// Gets the operation that owns this set of variables.
        /// </summary>
        /// <value>The owner operation.</value>
        IOperationDocumentPart Operation { get; }

        /// <summary>
        /// Gets a set of variable names that have been duplicated on the owner <see cref="Operation"/>.
        /// </summary>
        /// <value>The duplicates.</value>
        IEnumerable<string> Duplicates { get; }

        /// <summary>
        /// Gets the number of variables defined in this instance.
        /// </summary>
        /// <value>The number of defined variables.</value>
        int Count { get; }

        /// <summary>
        /// Gets a list of variables that have not been marked as referenced in the query document.
        /// </summary>
        /// <value>The unreferenced variables.</value>
        IEnumerable<IVariableDocumentPart> UnreferencedVariables { get; }

        /// <summary>
        /// Gets the <see cref="IVariableDocumentPart"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>IVariableDocumentPart.</returns>
        IVariableDocumentPart this[string name] { get; }
    }
}