﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// The default implementation of the cache key manager. This key manager is used unless overriden by
    /// a user's individual configuration.
    /// </summary>
    public class DefaultQueryExecutionPlanCacheKeyManager : IQueryExecutionPlanCacheKeyManager
    {
        /// <inheritdoc />
        public string CreateKey<TSchema>(string queryText, string operationName)
             where TSchema : class, ISchema
        {
            operationName = operationName?.Trim() ?? string.Empty;

            // tilde (~) isn't allowed in an operaiton name
            // making hte entry unique for a request that doesnt specify an operation name
            if (!string.IsNullOrEmpty(operationName))
                operationName = $"Operation-{operationName}";
            else
                operationName = "Operation~NONE";

            return $"{typeof(TSchema).Name}_{operationName}_{GraphQLParser.StripInsignificantWhiteSpace(queryText)}";
        }
    }
}