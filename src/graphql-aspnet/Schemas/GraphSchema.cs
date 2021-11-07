// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.TypeCollections;

    /// <summary>
    /// An object that can be used as a base for a custom schema or used directly as the only schema in a single schema setup.
    /// </summary>
    /// <seealso cref="ISchema" />
    [DebuggerDisplay("Default Schema (Known Types = {KnownTypes.Count})")]
    public class GraphSchema : ISchema
    {
        /// <summary>
        /// The human-friendly named assigned to the default graph schema type (this schema type).
        /// </summary>
        public const string DEFAULT_NAME = "-Default-";

        /// <summary>
        /// The human-friendly named assigned to the default graph schema type (this schema type).
        /// </summary>
        public const string DEFAULT_DESCRIPTION = "-Default Schema-";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchema"/> class.
        /// </summary>
        public GraphSchema()
        {
            this.OperationTypes = new Dictionary<GraphCollection, IGraphOperation>();
            this.KnownTypes = new SchemaTypeCollection();
            this.Configuration = new SchemaConfiguration();
        }

        /// <inheritdoc />
        public IDictionary<GraphCollection, IGraphOperation> OperationTypes { get; }

        /// <inheritdoc />
        public ISchemaTypeCollection KnownTypes { get; }

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public ISchemaConfiguration Configuration { get; }

        /// <inheritdoc />
        public virtual string Name => DEFAULT_NAME;

        /// <inheritdoc />
        public virtual string Description { get; } = DEFAULT_DESCRIPTION;
    }
}