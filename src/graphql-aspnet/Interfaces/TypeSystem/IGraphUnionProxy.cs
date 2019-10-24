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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An interface describing a proxy class which contains the metadata for a
    /// <see cref="UnionGraphType"/>.
    /// </summary>
    public interface IGraphUnionProxy
    {
        /// <summary>
        /// Gets the name of the union. This name will be subjected to schema configuration rules
        /// and will be altered accordingly when assigned to a schema.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a human readable description of this union type.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// Gets the types that belong in this union. These types will be automatically added to the
        /// schema. A minimum of 2 types are required.
        /// </summary>
        /// <value>The types.</value>
        HashSet<Type> Types { get; }

        /// <summary>
        /// Gets a value indicating whether this any union types created from this proxy
        /// are published in a schema introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        bool Publish { get; }
    }
}