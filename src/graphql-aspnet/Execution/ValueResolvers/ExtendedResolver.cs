// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.ValueResolvers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A resolver that can extend an exsiting resolver.
    /// </summary>
    [DebuggerDisplay("Extended Resolver")]
    public class ExtendedResolver : IGraphFieldResolver
    {
        private readonly IGraphFieldResolver _primaryResolver;
        private readonly Func<FieldResolutionContext, CancellationToken, Task> _extention;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedResolver" /> class.
        /// </summary>
        /// <param name="primaryResolver">The resolver to invoke.</param>
        /// <param name="extension">The extension to call after the primary
        /// resolver completes.</param>
        public ExtendedResolver(
            IGraphFieldResolver primaryResolver,
            Func<FieldResolutionContext, CancellationToken, Task> extension)
        {
            _primaryResolver = Validation.ThrowIfNullOrReturn(primaryResolver, nameof(primaryResolver));
            _extention = Validation.ThrowIfNullOrReturn(extension, nameof(extension));
        }

        /// <inheritdoc />
        public async Task Resolve(FieldResolutionContext context, CancellationToken cancelToken = default)
        {
            await _primaryResolver.Resolve(context, cancelToken).ConfigureAwait(false);
            await _extention.Invoke(context, cancelToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Type ObjectType => _primaryResolver.ObjectType;
    }
}