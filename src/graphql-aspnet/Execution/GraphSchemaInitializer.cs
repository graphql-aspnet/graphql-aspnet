// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// Perform a set of standardized steps to setup and configure any graph schema according to the rules
    /// for document operation execution used by the various schema pipelines.
    /// </summary>
    public class GraphSchemaInitializer
    {
        private readonly SchemaOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaInitializer" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public GraphSchemaInitializer(SchemaOptions options)
        {
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
        }

        /// <summary>
        /// Initializes the schema:
        /// <para>* Add any controllers to the schema instance that were configured during startup.</para>
        /// <para>* Add all methods, virtual graph types, return types parameters and property configurations.</para>
        /// <para>* Add any additional types added at startup.</para>
        /// <para>* Register introspection meta-fields.</para>
        /// </summary>
        /// <param name="schema">The schema to initialize.</param>
        public virtual void Initialize(ISchema schema)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            if (schema.IsInitialized)
                return;

            lock (schema)
            {
                if (schema.IsInitialized)
                    return;

                schema.Configuration.Merge(_options.CreateConfiguration());

                var manager = new GraphSchemaManager(schema);
                manager.AddIntrospectionFields();
                manager.AddBuiltInDirectives();

                // add any configured types to this instance
                foreach (var type in _options.RegisteredSchemaTypes)
                {
                    var typeDeclaration = type.SingleAttributeOrDefault<GraphTypeAttribute>();
                    if (typeDeclaration != null && typeDeclaration.PreventAutoInclusion)
                        continue;

                    manager.EnsureGraphType(type);
                }

                manager.BuildIntrospectionData();
                schema.IsInitialized = true;
            }
        }
    }
}