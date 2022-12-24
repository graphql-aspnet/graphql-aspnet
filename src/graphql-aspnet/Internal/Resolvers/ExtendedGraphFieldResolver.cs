﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.Resolvers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A resolver that can extend another field resolver with a custom function.
    /// </summary>
    [DebuggerDisplay("Extended Resolver")]
    internal class ExtendedGraphFieldResolver : IGraphFieldResolver
    {
        private readonly IGraphFieldResolver _primaryResolver;
        private readonly Func<FieldResolutionContext, CancellationToken, Task> _extension;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedGraphFieldResolver" /> class.
        /// </summary>
        /// <param name="primaryResolver">The resolver to invoke.</param>
        /// <param name="extension">The extension to call after the primary
        /// resolver completes.</param>
        public ExtendedGraphFieldResolver(
            IGraphFieldResolver primaryResolver,
            Func<FieldResolutionContext, CancellationToken, Task> extension)
        {
            _primaryResolver = Validation.ThrowIfNullOrReturn(primaryResolver, nameof(primaryResolver));
            _extension = Validation.ThrowIfNullOrReturn(extension, nameof(extension));
        }

        /// <inheritdoc />
        public async Task ResolveAsync(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            await _primaryResolver.ResolveAsync(context, cancelToken).ConfigureAwait(false);
            await _extension.Invoke(context, cancelToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Type ObjectType => _primaryResolver.ObjectType;
    }
}