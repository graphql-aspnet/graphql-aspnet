﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Methods for quickly configuring schema related things during testing.
    /// </summary>
    public static class SchemaHelperMethods
    {
        /// <summary>
        /// Alters the declaration options of the schema to ensure no fields or types are altered
        /// or have their names changed from the casing declared in the source code.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public static void SetNoAlterationConfiguration(this ISchema schema)
        {
            var declarationOptions = new SchemaDeclarationConfiguration();
            declarationOptions.Merge(schema.Configuration.DeclarationOptions);
            declarationOptions.GraphNamingFormatter = new GraphNameFormatter(GraphNameFormatStrategy.NoChanges);

            var config = new SchemaConfiguration(
                declarationOptions,
                schema.Configuration.ExecutionOptions,
                schema.Configuration.ResponseOptions,
                schema.Configuration.QueryCacheOptions);

            schema.Configuration.Merge(config);
        }

        /// <summary>
        /// Sets the schema configuration to allow the processing of subscriptions in test mode.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public static void SetSubscriptionAllowances(this ISchema schema)
        {
            schema.Configuration.DeclarationOptions.AllowedOperations.Add(GraphOperationType.Subscription);
        }
    }
}