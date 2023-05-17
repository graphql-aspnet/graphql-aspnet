// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A builder for performing advanced configuration of a schema's pipeline and processing settings.
    /// </summary>
    public interface ISchemaBuilder
    {
        /// <summary>
        /// Gets the completed options used to configure this schema.
        /// </summary>
        /// <value>The options used to create and configure this builder.</value>
        SchemaOptions Options { get; }
    }
}