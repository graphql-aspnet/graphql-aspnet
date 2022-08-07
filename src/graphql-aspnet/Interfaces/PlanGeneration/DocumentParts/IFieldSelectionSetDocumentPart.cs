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
    public interface IFieldSelectionSetDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Searches this selection set for any fields with the given output alias/name. Found
        /// fields may include those that would be included at the same level
        /// as this selection set from any inline fragments or named
        /// fragment spreads.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns>A list of fields with the given name or an empty list.</returns>
        IEnumerable<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias);

        /// <summary>
        /// Gets a set of fields to resolve for this selection set, in order of execution,
        /// combining all document parts that contribute to the set of fields to be resolved.
        /// This list walks any inline fragments and fragment spreads to produce a final set of fields
        /// that should be resolved.
        /// </summary>
        /// <value>The executable fields of this selection set.</value>
        IExecutableFieldSelectionSet ExecutableFields { get; }
    }
}