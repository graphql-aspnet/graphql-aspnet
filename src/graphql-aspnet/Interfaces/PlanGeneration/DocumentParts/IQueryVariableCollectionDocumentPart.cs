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
    public interface IQueryVariableCollectionDocumentPart : IReadOnlyDictionary<string, IQueryVariableDocumentPart>, IDocumentPart
    {
        /// <summary>
        /// Adds a parsed variable to this collection.
        /// </summary>
        /// <param name="variable">The variable to add.</param>
        internal void AddVariable(IQueryVariableDocumentPart variable);
    }
}