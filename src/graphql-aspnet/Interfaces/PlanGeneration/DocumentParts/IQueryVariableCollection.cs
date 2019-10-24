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
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// A representation of a collection of variables on a given operation defined in a user's query document.
    /// </summary>
    public interface IQueryVariableCollection : IReadOnlyDictionary<string, QueryVariable>, IDocumentPart
    {
        /// <summary>
        /// Adds a parsed variable to this collection.
        /// </summary>
        /// <param name="variable">The variable to add.</param>
        void AddVariable(QueryVariable variable);
    }
}