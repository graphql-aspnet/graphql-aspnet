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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// The complete result of turning a <see cref="IGraphFieldBaseTemplate"/> into a <see cref="IGraphField"/>.
    /// </summary>
    public class GraphFieldCreationResult : BaseItemDependencyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldCreationResult" /> class.
        /// </summary>
        public GraphFieldCreationResult()
        {
        }

        /// <summary>
        /// Gets or sets the generated graph type.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphField Field { get; set; }
    }
}