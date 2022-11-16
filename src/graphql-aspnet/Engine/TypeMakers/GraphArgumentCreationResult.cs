﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine.TypeMakers
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A pairing of a generated graph type with the concrete type its associcated with.
    /// </summary>
    public class GraphArgumentCreationResult : BaseItemDependencyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphArgumentCreationResult" /> class.
        /// </summary>
        public GraphArgumentCreationResult()
        {
        }

        /// <summary>
        /// Gets or sets the generated graph type.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphArgument Argument { get; set; }
    }
}