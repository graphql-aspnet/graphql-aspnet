// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An internal implementation of <see cref="IGraphUnionProxy"/> to use when
    /// a union is declared via attribution.
    /// </summary>
    public class GraphUnionProxy : IGraphUnionProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUnionProxy"/> class.
        /// </summary>
        /// <param name="unionName">Name of the union.</param>
        /// <param name="typesToInclude">The types to include.</param>
        public GraphUnionProxy(string unionName, IEnumerable<Type> typesToInclude)
        {
            this.Name = unionName;
            this.Description = null;
            this.Types = new HashSet<Type>(typesToInclude);
        }

        /// <summary>
        /// Gets the name of the union. This name will be subjected to schema configuration rules
        /// and will be altered accordingly when assigned to a schema.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets a human readable description of this union type.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Gets the types that belong in this union. These types will be automatically added to the
        /// schema. A minimum of 2 types are required.
        /// </summary>
        /// <value>The types.</value>
        public HashSet<Type> Types { get; }

        /// <summary>
        /// Gets a value indicating whether this any union types created from this proxy
        /// are published in a schema introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish => true;
    }
}