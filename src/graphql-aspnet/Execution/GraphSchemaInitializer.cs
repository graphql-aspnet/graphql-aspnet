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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Perform a set of standardized steps to setup and configure any graph schema according to the rules
    /// for document operation execution used by the various schema pipelines.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema that the initializer
    /// can work with.</typeparam>
    public class GraphSchemaInitializer<TSchema>
        where TSchema : class, ISchema
    {
        private readonly SchemaOptions _options;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaInitializer{TSchema}" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="serviceProvider">The service provider used to instantiate
        /// and apply type system directives.</param>
        public GraphSchemaInitializer(SchemaOptions options, IServiceProvider serviceProvider)
        {
            _options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            _serviceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Initializes the schema:
        /// <para>* Add any controllers to the schema instance that were configured during startup.</para>
        /// <para>* Add all methods, virtual graph types, return types parameters and property configurations.</para>
        /// <para>* Add any additional types added at startup.</para>
        /// <para>* Register introspection meta-fields.</para>
        /// </summary>
        /// <param name="schema">The schema to initialize.</param>
        public virtual void Initialize(TSchema schema)
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