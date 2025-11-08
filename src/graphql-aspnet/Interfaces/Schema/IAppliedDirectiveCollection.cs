// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Schema
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Directives;

    /// <summary>
    /// A collection of directives that have been applied to a schema
    /// item.
    /// </summary>
    public interface IAppliedDirectiveCollection : IEnumerable<IAppliedDirective>
    {
        /// <summary>
        /// Clones this collection and assigns the provided parent to it.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        /// <returns>AppliedDirectiveCollection.</returns>
        IAppliedDirectiveCollection Clone(ISchemaItem newParent);

        /// <summary>
        /// Performs a test to see if a directive of the given type is included in
        /// the list of applied directives. This method only indicates if a directive was included at least once.
        /// </summary>
        /// <typeparam name="TDirectiveType">The type of directive to check for.</typeparam>
        bool Includes<TDirectiveType>()
            where TDirectiveType : GraphDirective;

        /// <summary>
        /// Adds a new directive application to this collection.
        /// </summary>
        /// <param name="directive">The directive.</param>
        void Add(IAppliedDirective directive);

        /// <summary>
        /// Gets the parent schema item that owns this directive set.
        /// </summary>
        /// <value>The parent.</value>
        ISchemaItem Parent { get; }

        /// <summary>
        /// Gets the total number of directives in this collection.
        /// </summary>
        /// <value>The count of directives.</value>
        int Count { get; }
    }
}