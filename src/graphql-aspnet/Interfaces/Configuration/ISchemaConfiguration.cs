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
    /// <summary>
    /// A schema configuration, detailing all the operations for executing graph queries for a given schema type.
    /// </summary>
    public interface ISchemaConfiguration
    {
        /// <summary>
        /// Gets the options related to the runtime declaration characteristics of this schema.
        /// </summary>
        /// <value>The schema declaration configuration options.</value>
        ISchemaDeclarationConfiguration DeclarationOptions { get; }

        /// <summary>
        /// Gets options related to the processing of user queries at runtime.
        /// </summary>
        /// <value>The execution options.</value>
        ISchemaExecutionConfiguration ExecutionOptions { get; }

        /// <summary>
        /// Gets the options related to the formatting of a graphql respomnse before it is sent to the requestor.
        /// </summary>
        /// <value>The schema reponse configuration options.</value>
        ISchemaResponseConfiguration ResponseOptions { get; }

        /// <summary>
        /// Gets the options related to how this schema caches and evicts query plans at runtime.
        /// </summary>
        /// <value>The query cache options.</value>
        ISchemaQueryPlanCacheConfiguration QueryCacheOptions { get; }

        /// <summary>
        /// Merges the specified schema configuration into this configuration as allowed by this instance.
        /// </summary>
        /// <param name="schemaConfig">The schema configuration.</param>
        void Merge(ISchemaConfiguration schemaConfig);
    }
}