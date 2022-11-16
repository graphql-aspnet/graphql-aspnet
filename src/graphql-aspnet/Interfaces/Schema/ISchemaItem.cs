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
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An item that is part of a <see cref="ISchema"/>.
    /// </summary>
    public interface ISchemaItem : INamedItem
    {
        /// <summary>
        /// Gets the unique route string assigned to this item
        /// in the object graph.
        /// </summary>
        /// <value>The route.</value>
        SchemaItemPath Route { get; }

        /// <summary>
        /// Gets a collection of directives applied to this schema item
        /// when it was instantiated in a schema.
        /// </summary>
        /// <value>The directives.</value>
        IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}