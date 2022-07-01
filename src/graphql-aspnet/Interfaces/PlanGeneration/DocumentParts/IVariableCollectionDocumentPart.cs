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

        void MarkAsReferenced(string variableName);

        bool IsReferenced(string variableName);

        void ClearReferences();


        bool Contains(string key);


        bool TryGetValue(string key, out IVariableDocumentPart value);

        /// <summary>
        /// Gets the operation that defines these variables.
        /// </summary>
        /// <value>The owner operation.</value>
        IOperationDocumentPart Operation { get; }

        IEnumerable<string> Duplicates { get; }

        int Count {get;}

        IEnumerable<IVariableDocumentPart> UnreferencedVariables { get; }

        IVariableDocumentPart this[string key] { get; }
    }
}