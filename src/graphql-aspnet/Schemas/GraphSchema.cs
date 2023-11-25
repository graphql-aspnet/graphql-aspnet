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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.TypeCollections;

    /// <summary>
    /// An object that can be used as a base for a custom schema or used directly as the only schema
    /// in a single schema setup.
    /// </summary>
    /// <seealso cref="ISchema" />
    [DebuggerDisplay("Default Schema (Known Types = {KnownTypes.Count})")]
    public class GraphSchema : ISchema
    {
        /// <summary>
        /// The human-friendly named assigned to the default graph schema type.
        /// </summary>
        public const string DEFAULT_NAME = "-Default-";

        /// <summary>
        /// The human-friendly named assigned to the default graph schema type.
        /// </summary>
        public const string DEFAULT_DESCRIPTION = "-Default Schema-";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchema"/> class.
        /// </summary>
        public GraphSchema()
        {
            this.Operations = new Dictionary<GraphOperationType, IGraphOperation>();
            this.KnownTypes = new SchemaTypeCollection();
            this.Configuration = new SchemaConfiguration();
            this.AppliedDirectives = new AppliedDirectiveCollection(this);

            var graphName = this.GetType().FriendlyGraphTypeName();
            if (!GraphValidation.IsValidGraphName(graphName))
            {
                throw new GraphTypeDeclarationException(
                    $"The type {this.GetType().FriendlyName()} cannot be used as a " +
                    $"schema due to its C# type name. Ensure all schema types have no " +
                    $"special characters (such as carrots for generics) and does not start with an underscore.");
            }

            this.Route = new SchemaItemPath(SchemaItemPathCollections.Schemas, graphName);
            this.Name = DEFAULT_NAME;
            this.InternalName = this.GetType().FriendlyName();
            this.Description = DEFAULT_DESCRIPTION;
        }

        /// <inheritdoc />
        public IDictionary<GraphOperationType, IGraphOperation> Operations { get; }

        /// <inheritdoc />
        public ISchemaTypeCollection KnownTypes { get; }

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public ISchemaConfiguration Configuration { get; }

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public virtual string InternalName { get; set; }

        /// <inheritdoc />
        public virtual string Description { get; set; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }
    }
}