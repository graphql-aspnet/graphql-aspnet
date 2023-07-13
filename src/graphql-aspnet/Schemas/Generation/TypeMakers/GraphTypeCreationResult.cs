// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A pairing of a generated graph type with the concrete type its associcated with.
    /// </summary>
    [DebuggerDisplay("Type: {GraphType.Name}")]
    public class GraphTypeCreationResult : DependentTypeCollection
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