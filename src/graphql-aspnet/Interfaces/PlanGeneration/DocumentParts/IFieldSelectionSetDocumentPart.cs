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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A collection of fields to query <see cref="IFieldDocumentPart"/>
    /// from a source object.
    /// </summary>
    public interface IFieldSelectionSetDocumentPart : IReadOnlyList<IFieldDocumentPart>, IDocumentPart
    {
        /// <summary>
        /// Returns a value indicating if any field in this instance matches the given alias.
        /// This method is a fast lookup and does not traverse the list of nodes.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns><c>true</c> if the alias was found,; otherwise, <c>false</c>.</returns>
        bool ContainsAlias(in ReadOnlyMemory<char> alias);

        /// <summary>
        /// Searches this selection set for any fields with the given output alias/name.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns>A list of fields with the given name or an empty list.</returns>
        IEnumerable<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias);
    }
}