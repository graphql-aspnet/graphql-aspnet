// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Response
{
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A set of options for configuring the serialized contents of a <see cref="IGraphOperationResult"/> that is to be sent to a client.
    /// </summary>
    public class GraphQLResponseOptions
    {
        /// <summary>
        /// Gets a default set of options to use when none are supplied to a response writer.
        /// </summary>
        /// <value>The default.</value>
        public static GraphQLResponseOptions Default { get; }

        /// <summary>
        /// Initializes static members of the <see cref="GraphQLResponseOptions"/> class.
        /// </summary>
        static GraphQLResponseOptions()
        {
            Default = new GraphQLResponseOptions()
            {
                ExposeMetrics = false,
                ExposeExceptions = false,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLResponseOptions"/> class.
        /// </summary>
        public GraphQLResponseOptions()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether any metrics data contained in a response should be written to the package sent to a client.
        /// </summary>
        /// <value><c>true</c> if metrics data should be exposed; otherwise, <c>false</c>.</value>
        public bool ExposeMetrics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions contained in a response should be written to the package sent to a client.
        /// </summary>
        /// <value><c>true</c> if exception data should be exposed; otherwise, <c>false</c>.</value>
        public bool ExposeExceptions { get; set; }
    }
}