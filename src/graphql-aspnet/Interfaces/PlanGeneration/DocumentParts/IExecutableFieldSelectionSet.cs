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
    /// A set of fields to be executed within a given
    /// <see cref="IFieldSelectionSetDocumentPart"/>. This executable set includes
    /// all the fields that would end up in the results document for this selection set which
    /// includes fields that would be added via an inline fragment or a
    /// spread of a named fragment.
    /// </summary>
    public interface IExecutableFieldSelectionSet : IEnumerable<IFieldDocumentPart>
    {
        /// <summary>
        /// Filters the set of executable fields to those that match the provided alias.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns>IReadOnlyList&lt;IFieldDocumentPart&gt;.</returns>
        IEnumerable<IFieldDocumentPart> FilterByAlias(string alias);

        /// <summary>
        /// Gets the subset of executable fields that are marked as being included
        /// in a query result.
        /// </summary>
        /// <value>The included fields only.</value>
        IEnumerable<IFieldDocumentPart> IncludedOnly { get; }

        /// <summary>
        /// Gets the field selection set that owns this executable set.
        /// </summary>
        /// <value>The owner.</value>
        IFieldSelectionSetDocumentPart Owner { get; }

        /// <summary>
        /// Gets or sets the <see cref="IFieldDocumentPart"/> at the specified index. This index
        /// represents its position in the resultant field. This may be a field in a child
        /// fragment spread or inline fragment and will not necessarily coorispond to a direct child
        /// of the owner selection set.
        /// </summary>
        /// <remarks>
        /// This method works in the same capacity as as <c>ElementAt()</c> and
        /// exists only for convience reasons. Avoid using it for performance critical operations.</remarks>
        /// <param name="index">The index of the field.</param>
        /// <returns>IFieldDocumentPart.</returns>
        IFieldDocumentPart this[int index] { get; }
    }
}