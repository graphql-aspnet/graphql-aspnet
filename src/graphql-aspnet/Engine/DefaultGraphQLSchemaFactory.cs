// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Startup;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema being built.</typeparam>
    public partial class DefaultGraphQLSchemaFactory<TSchema> : IGraphQLSchemaFactory<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLSchemaFactory{TSchema}"/> class.
        /// </summary>
        public DefaultGraphQLSchemaFactory()
            : this(true, true, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQLSchemaFactory{TSchema}" /> class.
        /// </summary>
        /// <param name="includeBuiltInDirectives">if set to <c>true</c> the specification defined
        /// directives (e.g. @skip, @include etc.) will be automatically added to any
        /// created schema instance.</param>
        /// <param name="processTypeSystemDirectives">if set to <c>true</c> any discovered
        /// type system directives will be applied to their target schema items.</param>
        /// <param name="factory">A type factory instance to use. When null, one will attempt to be created
        /// from the schema's service scope.</param>
        public DefaultGraphQLSchemaFactory(
            bool includeBuiltInDirectives = true,
            bool processTypeSystemDirectives = true,
            IGraphQLTypeMakerFactory<TSchema> factory = null)
        {
            this.IncludeBuiltInDirectives = includeBuiltInDirectives;
            this.ProcessTypeSystemDirectives = processTypeSystemDirectives;
            this.MakerFactory = factory;
        }

        /// <inheritdoc />
        public virtual TSchema CreateInstance(
            IServiceScope serviceScope,
            ISchemaConfiguration configuration,
            IEnumerable<SchemaTypeToRegister> typesToRegister = null,
            IEnumerable<IGraphQLRuntimeSchemaItemDefinition> runtimeItemDefinitions = null,
            IEnumerable<IGraphQLServerExtension> schemaExtensions = null)
        {
            Validation.ThrowIfNull(serviceScope, nameof(serviceScope));
            Validation.ThrowIfNull(configuration, nameof(configuration));

            this.ServiceProvider = serviceScope.ServiceProvider;

            runtimeItemDefinitions = runtimeItemDefinitions ?? Enumerable.Empty<IGraphQLRuntimeSchemaItemDefinition>();
            typesToRegister = typesToRegister ?? Enumerable.Empty<SchemaTypeToRegister>();
            schemaExtensions = schemaExtensions ?? Enumerable.Empty<IGraphQLServerExtension>();

            this.Schema = GraphSchemaBuilder.BuildSchema<TSchema>(this.ServiceProvider);

            if (this.Schema.IsInitialized)
                return this.Schema;

            this.Schema.Configuration.Merge(configuration);
            if (this.MakerFactory == null)
                this.MakerFactory = this.ServiceProvider.GetRequiredService<IGraphQLTypeMakerFactory<TSchema>>();

            this.MakerFactory.Initialize(this.Schema);

            // Step 1: Ensure all the bare bones requirements are set
            // --------------------------------------
            this.EnsureBaseLineDependencies();

            // Step 2a: Figure out which types are scalars and register them first
            // --------------------------------------
            // This speeds up the generation process since they don't have to be found
            // and validated when trying to import objects or interfaces.
            var scalarsToRegister = new List<SchemaTypeToRegister>();
            var nonScalarsToRegister = new List<SchemaTypeToRegister>();
            foreach (var regType in typesToRegister)
            {
                if (Validation.IsCastable<IScalarGraphType>(GlobalTypes.FindBuiltInScalarType(regType.Type) ?? regType.Type))
                    scalarsToRegister.Add(regType);
                else
                    nonScalarsToRegister.Add(regType);
            }

            // Step 2b: Register all explicitly declared scalars first
            // --------------------------------------
            foreach (var type in scalarsToRegister)
                this.EnsureGraphType(type.Type);

            // Step 2c: Register other graph types
            // --------------------------------------
            foreach (var type in nonScalarsToRegister)
                this.EnsureGraphType(type.Type, type.TypeKind);

            // Step 3: Register any runtime defined items (e.g. minimal api registrations)
            // --------------------------------------
            foreach (var itemDef in runtimeItemDefinitions)
                this.AddRuntimeSchemaItemDefinition(itemDef);

            // Step 4: execute any assigned schema extensions
            // --------------------------------------
            // this includes any late bound directives added to the type system via .ApplyDirective()
            foreach (var extension in schemaExtensions)
                extension.EnsureSchema(this.Schema);

            // Step 5: apply all queued type system directives to the now filled schema
            // --------------------------------------
            if (this.ProcessTypeSystemDirectives)
                this.ApplyTypeSystemDirectives();

            // Step 6: Run final validations to ensure the schema is internally consistant
            // --------------------------------------
            this.ValidateSchemaIntegrity();

            // Step 7: Rebuild introspection data to match the now completed schema instance
            // --------------------------------------
            if (!this.Schema.Configuration.DeclarationOptions.DisableIntrospection)
                this.RebuildIntrospectionData();

            this.Schema.IsInitialized = true;
            return this.Schema;
        }

        /// <summary>
        /// Ensures the target schema has all the "specification required" pieces and dependencies
        /// accounted for.
        /// </summary>
        protected virtual void EnsureBaseLineDependencies()
        {
            // all schemas depend on String because of the __typename field
            this.EnsureGraphType(typeof(string));

            // ensure top level schema directives are accounted for
            foreach (var directive in this.Schema.GetType().ExtractAppliedDirectives())
            {
                this.Schema.AppliedDirectives.Add(directive);
            }

            foreach (var appliedDirective in this.Schema.AppliedDirectives.Where(x => x.DirectiveType != null))
            {
                this.EnsureGraphType(
                    appliedDirective.DirectiveType,
                    TypeKind.DIRECTIVE);
            }

            // ensure all globally required directives are added
            if (this.IncludeBuiltInDirectives)
            {
                foreach (var type in Constants.GlobalDirectives)
                    this.EnsureGraphType(type);
            }

            // all schemas must support query
            this.EnsureGraphOperationType(GraphOperationType.Query);
        }

        /// <summary>
        /// Gets the service provider that will be used to create service instances
        /// needed to generate the schema
        /// </summary>
        /// <value>The service provider.</value>
        protected virtual IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the configuration settings that will be used to generate
        /// the schema.
        /// </summary>
        /// <value>The schema configuration instance.</value>
        protected virtual ISchemaConfiguration Configuration => this.Schema.Configuration;

        /// <summary>
        /// Gets the schema instance being built by this factory instance.
        /// </summary>
        /// <value>The schema.</value>
        protected virtual TSchema Schema { get; private set; }

        /// <summary>
        /// Gets or sets a factory instnace that can serve up instances of various
        /// makers to generate graph types for the building <see cref="Schema"/>.
        /// </summary>
        /// <value>The maker factory to use in this instance.</value>
        protected virtual IGraphQLTypeMakerFactory MakerFactory { get; set; }

        /// <summary>
        /// Gets a value indicating whether the specification-defined, built-in directives (e.g. skip, include etc.)
        /// are automatically added to the schema that is generated.
        /// </summary>
        /// <value><c>true</c> if [include built in directives]; otherwise, <c>false</c>.</value>
        protected virtual bool IncludeBuiltInDirectives { get; }

        /// <summary>
        /// Gets a value indicating whether any assigned type system directives will be processed when a schema instance
        /// is built.
        /// </summary>
        /// <value><c>true</c> if type system directives should be processed for any schema items; otherwise, <c>false</c>.</value>
        protected virtual bool ProcessTypeSystemDirectives { get; }
    }
}