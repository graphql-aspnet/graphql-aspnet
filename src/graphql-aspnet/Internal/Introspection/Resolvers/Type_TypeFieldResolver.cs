// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Resolvers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A resolver to extract fields from a __Type during an introspection query.
    /// </summary>
    internal class Type_TypeFieldResolver : IGraphFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Type_TypeFieldResolver" /> class.
        /// </summary>
        public Type_TypeFieldResolver()
        {
        }

        /// <summary>
        /// Executes the given context in an attempt to resolve the request and produce a reslt.
        /// </summary>
        /// <param name="context">The field context containing the necessary data to resolve
        /// the field and produce a reslt.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var sourceData = context.Arguments.SourceData as IntrospectedType;

            if (sourceData == null)
            {
                context.Result = Enumerable.Empty<IntrospectedField>();
            }
            else
            {
                var includedDeprecated = context.Arguments.ContainsKey(Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME)
                                         && (bool)context.Arguments[Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME].Value;

                if (includedDeprecated)
                    context.Result = sourceData.Fields;
                else
                    context.Result = sourceData.Fields?.Where(x => !x.IsDeprecated).ToList();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create as a during its invocation. If this
        /// resolver may generate a list, this type should represent a single list item. (i.e. 'string' not 'List[string]').
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => typeof(IntrospectedField);
    }
}