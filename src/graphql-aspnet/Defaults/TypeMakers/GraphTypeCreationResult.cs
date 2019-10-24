// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults.TypeMakers
{
    using System;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A pairing of a generated graph type with the concrete type its associcated with.
    /// </summary>
    public class GraphTypeCreationResult : BaseItemDependencyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeCreationResult" /> class.
        /// </summary>
        public GraphTypeCreationResult()
        {
        }

        /// <summary>
        /// Gets or sets the generated graph type.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphType GraphType { get; set; }

        /// <summary>
        /// Gets or sets the concrete type associated with the <see cref="GraphType"/>.
        /// </summary>
        /// <value>The type of the concrete.</value>
        public Type ConcreteType { get; set; }
    }
}