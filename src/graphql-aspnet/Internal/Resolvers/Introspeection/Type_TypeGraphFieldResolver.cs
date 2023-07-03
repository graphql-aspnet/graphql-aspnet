// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers.Introspeection
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// A resolver to extract fields from a __Type during an introspection query.
    /// </summary>
    internal class Type_TypeGraphFieldResolver : IGraphFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Type_TypeGraphFieldResolver" /> class.
        /// </summary>
        public Type_TypeGraphFieldResolver()
        {
        }

        /// <inheritdoc />
        public Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
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
    }
}