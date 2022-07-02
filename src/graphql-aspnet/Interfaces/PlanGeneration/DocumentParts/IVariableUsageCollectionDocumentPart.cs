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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    public interface IVariableUsageCollectionDocumentPart : IEnumerable<IVariableUsageDocumentPart>
    {
        /// <summary>
        /// Determines whether the specified variable is used by any item in this collection.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns><c>true</c> if the specified variable name has references; otherwise, <c>false</c>.</returns>
        bool HasUsages(string variableName);

        /// <summary>
        /// Finds the set of <see cref="IVariableUsageDocumentPart"/> for the
        /// variable of the given name.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>IEnumerable&lt;IVariableReferenceDocumentPart&gt;.</returns>
        IEnumerable<IVariableUsageDocumentPart> FindReferences(string variableName);

        /// <summary>
        /// Gets the total number of variable references in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets the owner of this collection.
        /// </summary>
        /// <value>The owner.</value>
        IDocumentPart Owner { get; }
    }
}