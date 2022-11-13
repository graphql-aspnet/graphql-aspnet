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
    public interface IExecutableFieldSelectionSet : IReadOnlyList<IFieldDocumentPart>
    {
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
    }
}