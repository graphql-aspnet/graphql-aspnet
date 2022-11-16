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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An interface describing a proxy class which contains the metadata for a
    /// <see cref="UnionGraphType"/>.
    /// </summary>
    public interface IGraphUnionProxy : IUnionTypeMapper, INamedItem
    {
        /// <summary>
        /// Gets the types that belong in this union. These types will be automatically added to the
        /// schema. A minimum of 2 types are required.
        /// </summary>
        /// <value>The types.</value>
        HashSet<Type> Types { get; }

        /// <summary>
        /// Gets or sets a value indicating whether any union types created from this proxy
        /// are published in a schema introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        bool Publish { get; set; }
    }
}