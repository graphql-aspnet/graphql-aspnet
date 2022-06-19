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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection of fields to query <see cref="IFieldSelectionDocumentPart"/>
    /// from a source object.
    /// </summary>
    public interface IFieldSelectionSetDocumentPart : IReadOnlyList<IFieldSelectionDocumentPart>, IDocumentPart
    {
        /// <summary>
        /// Adds the field selection to this instance.
        /// </summary>
        /// <param name="newField">The new field.</param>
        internal void AddFieldSelection(IFieldSelectionDocumentPart newField);

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
        IEnumerable<IFieldSelectionDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias);

        /// <summary>
        /// Gets the graph type of the expected source object, from which the requested fields
        /// in this set will be extracted.
        /// </summary>
        /// <value>The graph type definition for the source object.</value>
        public IGraphType SourceGraphType { get; }

        /// <summary>
        /// Gets the path in the document that leads to this selection set.
        /// </summary>
        /// <value>The field selecton path.</value>
        SourcePath Path { get; }
    }
}