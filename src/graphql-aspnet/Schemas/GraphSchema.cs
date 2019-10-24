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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.TypeCollections;

    /// <summary>
    /// An object that can be used as a base for a custom schema or as the only schema in a single schema setup
    /// that provides functionality for using <see cref="GraphController"/> as fields in an object graph.
    /// </summary>
    /// <seealso cref="ISchema" />
    public class GraphSchema : ISchema
    {
        /// <summary>
        /// The human-friendly named assigned to the default graph schema type (this schema type).
        /// </summary>
        public const string DEFAULT_NAME = "-Default-";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchema"/> class.
        /// </summary>
        public GraphSchema()
        {
            this.OperationTypes = new Dictionary<GraphCollection, IGraphOperation>();
            this.KnownTypes = new SchemaTypeCollection();
            this.Configuration = new SchemaConfiguration();
        }

        /// <summary>
        /// Gets the root operation types supported by this schema. (query, mutation etc.)
        /// </summary>
        /// <value>The root operation types.</value>
        public IDictionary<GraphCollection, IGraphOperation> OperationTypes { get; }

        /// <summary>
        /// Gets a collection of known graph object/scalar types available to this schema. Serves as the basis for the type
        /// system delivered on introspection requests as well as input value translation. <see cref="IGraphType"/> discovered
        /// as part of a <see cref="GraphController"/> registration are automatically included in this collection.
        /// </summary>
        /// <value>A collection of types, keyed on name, of the types registered and allowed on this schema.</value>
        public ISchemaTypeCollection KnownTypes { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is fully setup and usable. This value is used to determine
        /// if the runtime needs to parse and add controller fields to this schema instance.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Gets the a common, friendly name for the schema. This name may appear in error messages.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name => DEFAULT_NAME;

        /// <summary>
        /// Gets the configuration options that governs the handling of this schema by graphql.
        /// </summary>
        /// <value>The configuration.</value>
        public ISchemaConfiguration Configuration { get; }
    }
}