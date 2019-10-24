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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A resolver to extract enum values from a __Type during an introspection query.
    /// </summary>
    internal class Type_EnumValuesResolver : IGraphFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Type_EnumValuesResolver" /> class.
        /// </summary>
        public Type_EnumValuesResolver()
        {
        }

        /// <summary>
        /// Processes the given <see cref="IGraphFieldRequest" /> against this instance
        /// performing the operation as defined by this entity and generating a response.
        /// </summary>
        /// <param name="resolutionContext">The resolution context containing the request and the
        /// runtime information needed to resolve it.</param>
        /// <param name="cancelToken">The cancel token monitoring the execution of a graph request.</param>
        /// <returns>Task&lt;IGraphPipelineResponse&gt;.</returns>
        public Task Resolve(FieldResolutionContext resolutionContext, CancellationToken cancelToken = default)
        {
            var sourceData = resolutionContext.Arguments.SourceData as IntrospectedType;

            if (sourceData == null)
            {
                resolutionContext.Result = Enumerable.Empty<IntrospectedEnumValue>();
            }
            else
            {
                var includedDeprecated = resolutionContext.Arguments.ContainsKey(Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME)
                                         && (bool)resolutionContext.Arguments[Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME].Value;

                if (includedDeprecated)
                    resolutionContext.Result = sourceData.EnumValues;
                else
                    resolutionContext.Result = sourceData.EnumValues?.Where(x => !x.IsDeprecated).ToList();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the concrete type this resolver attempts to create as a during its invocation. If this
        /// resolver may generate a list, this type should represent a single list item. (i.e. 'string' not 'List[string]').
        /// </summary>
        /// <value>The type of the return.</value>
        public Type ObjectType => typeof(IntrospectedEnumValue);
    }
}