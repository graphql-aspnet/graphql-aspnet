// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// A configuration implementation containing all the applicable runtime options for any given schema.
    /// </summary>
    public class SchemaConfiguration : ISchemaConfiguration
    {
        private readonly SchemaDeclarationConfiguration _declarationOptions;
        private readonly SchemaExecutionConfiguration _executionOptions;
        private readonly SchemaResponseConfiguration _responseOptions;
        private readonly SchemaQueryPlanCacheConfiguration _cacheOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaConfiguration" /> class.
        /// </summary>
        /// <param name="declarationOptions">The declaration options to build the configuration with.</param>
        /// <param name="executionOptions">The execution options to build the configuration with.</param>
        /// <param name="responseOptions">The response options to build the configuration with.</param>
        /// <param name="cacheOptions">The cache options to build this configuration with.</param>
        public SchemaConfiguration(
            ISchemaDeclarationConfiguration declarationOptions = null,
            ISchemaExecutionConfiguration executionOptions = null,
            ISchemaResponseConfiguration responseOptions = null,
            ISchemaQueryPlanCacheConfiguration cacheOptions = null)
        {
            _declarationOptions = new SchemaDeclarationConfiguration();
            _executionOptions = new SchemaExecutionConfiguration();
            _responseOptions = new SchemaResponseConfiguration();
            _cacheOptions = new SchemaQueryPlanCacheConfiguration();

            _declarationOptions.Merge(declarationOptions);
            _executionOptions.Merge(executionOptions);
            _responseOptions.Merge(responseOptions);
            _cacheOptions.Merge(cacheOptions);
        }

        /// <summary>
        /// Merges the specified schema configuration into this configuration as allowed by this instance.
        /// </summary>
        /// <param name="schemaConfig">The schema configuration.</param>
        public void Merge(ISchemaConfiguration schemaConfig)
        {
            _declarationOptions.Merge(schemaConfig.DeclarationOptions);
            _executionOptions.Merge(schemaConfig.ExecutionOptions);
            _responseOptions.Merge(schemaConfig.ResponseOptions);
            _cacheOptions.Merge(schemaConfig.QueryCacheOptions);
        }

        /// <summary>
        /// Gets the options related to the runtime declaration characteristics of this schema.
        /// </summary>
        /// <value>The schema declaration configuration options.</value>
        public ISchemaDeclarationConfiguration DeclarationOptions => _declarationOptions;

        /// <summary>
        /// Gets options related to the processing of user queries at runtime.
        /// </summary>
        /// <value>The execution options.</value>
        public ISchemaExecutionConfiguration ExecutionOptions => _executionOptions;

        /// <summary>
        /// Gets the options related to the formatting of a graphql respomnse before it is sent to the requestor.
        /// </summary>
        /// <value>The schema reponse configuration options.</value>
        public ISchemaResponseConfiguration ResponseOptions => _responseOptions;

        /// <summary>
        /// Gets the options related to how this schema caches and evicts query plans at runtime.
        /// </summary>
        /// <value>The query cache options.</value>
        public ISchemaQueryPlanCacheConfiguration QueryCacheOptions => _cacheOptions;
    }
}