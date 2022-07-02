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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// A collection of fields to query <see cref="IFieldDocumentPart"/>
    /// from a source object.
    /// </summary>
    public interface IFieldSelectionSetDocumentPart : IReadOnlyList<IFieldDocumentPart>, IDocumentPart
    {
        /// <summary>
        /// Searches this selection set for any fields with the given output alias/name.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns>A list of fields with the given name or an empty list.</returns>
        IReadOnlyList<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias);
    }
}