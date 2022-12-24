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
    /// A resolver to extract enum values from a __Type during an introspection query.
    /// </summary>
    internal class Type_EnumValuesGraphFieldResolver : IGraphFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Type_EnumValuesGraphFieldResolver" /> class.
        /// </summary>
        public Type_EnumValuesGraphFieldResolver()
        {
        }

        /// <inheritdoc />
        public Task ResolveAsync(FieldResolutionContext resolutionContext, CancellationToken cancelToken = default)
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

        /// <inheritdoc />
        public Type ObjectType => typeof(IntrospectedEnumValue);
    }
}