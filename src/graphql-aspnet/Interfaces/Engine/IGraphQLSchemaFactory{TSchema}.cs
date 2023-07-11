// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory that, for the given schema type, can generate a fully qualified and usable
    /// schema instance.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this factory will generate.</typeparam>
    public interface IGraphQLSchemaFactory<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Creates a new, fully populated instance of the schema
        /// </summary>
        /// <param name="serviceScope">The service scope used to generate service
        /// instances when needed during schema generation.</param>
        /// <param name="configuration">The configuration options
        /// that will govern how the schema instantiated.</param>
        /// <param name="typesToRegister">The explicit types register
        /// on the schema.</param>
        /// <param name="runtimeItemDefinitions">The runtime field and type definitions (i.e. minimal api) to add to the schema.</param>
        /// <param name="schemaExtensions">The schema extensions to apply to the schema.</param>
        /// <returns>The completed schema instance.</returns>
        TSchema CreateInstance(
            IServiceScope serviceScope,
            ISchemaConfiguration configuration,
            IEnumerable<SchemaTypeToRegister> typesToRegister = null,
            IEnumerable<IGraphQLRuntimeSchemaItemDefinition> runtimeItemDefinitions = null,
            IEnumerable<ISchemaExtension> schemaExtensions = null);
    }
}