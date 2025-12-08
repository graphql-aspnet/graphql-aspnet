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
    /// A resolver to extract arguments from a __Field during an introspection query.
    /// </summary>
    internal class Field_ArgsGraphFieldResolver : IGraphFieldResolver
    {
        /// <inheritdoc />
        public Task ResolveAsync(FieldResolutionContext resolutionContext, CancellationToken cancelToken = default)
        {
            var sourceData = resolutionContext.Arguments.SourceData as IntrospectedField;

            if (sourceData is null)
            {
                resolutionContext.Result = Enumerable.Empty<IntrospectedInputValueType>();
            }
            else
            {
                var includedDeprecated = resolutionContext.Arguments.ContainsKey(Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME)
                                         && (bool)resolutionContext.Arguments[Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME].Value;

                if (includedDeprecated)
                    resolutionContext.Result = sourceData.Arguments;
                else
                    resolutionContext.Result = sourceData.Arguments?.Where(x => !x.IsDeprecated).ToList();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Type ObjectType => typeof(IntrospectedInputValueType);
    }
}