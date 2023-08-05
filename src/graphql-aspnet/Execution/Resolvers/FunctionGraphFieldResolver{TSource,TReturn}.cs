﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Resolvers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// Wraps a user provided function as a custom resolver to any declared field. Useful for small,
    /// mostly static resolution setups and ignores much of the passed data available to fully qualified resolvers.
    /// </summary>
    /// <typeparam name="TSource">The expected type of the source data.</typeparam>
    /// <typeparam name="TReturn">The expected type of the returned data.</typeparam>
    /// <remarks>
    /// This resolver is used heavily by the introspection system for simple, static data resolution
    /// and extending other more involved resolvers with simple add-on functionality.
    /// </remarks>
    internal class FunctionGraphFieldResolver<TSource, TReturn> : IGraphFieldResolver
        where TSource : class
    {
        private readonly Func<TSource, Task<TReturn>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionGraphFieldResolver{TSource,TReturn}" /> class.
        /// </summary>
        /// <param name="func">The function to execute to resolve the field.</param>
        public FunctionGraphFieldResolver(Func<TSource, Task<TReturn>> func)
        {
            _func = Validation.ThrowIfNullOrReturn(func, nameof(func));
            this.MetaData = InternalFieldResolverMetaData.CreateMetadata(this.GetType());
        }

        /// <inheritdoc />
        public async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            var data = await _func(context?.SourceData as TSource).ConfigureAwait(false);
            context.Result = data;
        }

        /// <inheritdoc />
        public IGraphFieldResolverMetaData MetaData { get; }
    }
}