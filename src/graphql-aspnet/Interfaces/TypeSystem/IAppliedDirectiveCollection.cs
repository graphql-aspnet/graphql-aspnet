// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of directives that have been applied to a schema
    /// item.
    /// </summary>
    public interface IAppliedDirectiveCollection : IReadOnlyList<IAppliedDirective>
    {
        /// <summary>
        /// Clones this collection and assigns the provided parent to it.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        /// <returns>AppliedDirectiveCollection.</returns>
        IAppliedDirectiveCollection Clone(ISchemaItem newParent);

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
    }
}