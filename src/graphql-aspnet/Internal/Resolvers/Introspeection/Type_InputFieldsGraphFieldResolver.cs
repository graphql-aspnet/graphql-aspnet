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
    /// A resolver to extract input fields from a __Type during an introspection query.
    /// </summary>
    internal class Type_InputFieldsGraphFieldResolver : IGraphFieldResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Type_InputFieldsGraphFieldResolver" /> class.
        /// </summary>
        public Type_InputFieldsGraphFieldResolver()
        {
        }

        /// <inheritdoc />
        public Task ResolveAsync(FieldResolutionContext resolutionContext, CancellationToken cancelToken = default)
        {
            var sourceData = resolutionContext.Arguments.SourceData as IntrospectedType;

            if (sourceData is null)
            {
                resolutionContext.Result = Enumerable.Empty<IntrospectedInputValueType>();
            }
            else
            {
                var includedDeprecated = resolutionContext.Arguments.ContainsKey(Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME)
                                         && (bool)resolutionContext.Arguments[Constants.ReservedNames.DEPRECATED_ARGUMENT_NAME].Value;

                if (includedDeprecated)
                    resolutionContext.Result = sourceData.InputFields;
                else
                    resolutionContext.Result = sourceData.InputFields?.Where(x => !x.IsDeprecated).ToList();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Type ObjectType => typeof(IntrospectedInputValueType);
    }
}