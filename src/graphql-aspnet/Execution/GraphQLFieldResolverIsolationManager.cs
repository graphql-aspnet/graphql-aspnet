// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// A default implementation of the resolver isolation manager, wrapping a simple
    /// <see cref="SemaphoreSlim"/>.
    /// </summary>
    public class GraphQLFieldResolverIsolationManager : IGraphQLFieldResolverIsolationManager, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLFieldResolverIsolationManager" /> class.
        /// </summary>
        public GraphQLFieldResolverIsolationManager()
        {
            _semaphore = new SemaphoreSlim(1);
        }

        /// <inheritdoc />
        public async Task WaitAsync()
        {
            await _semaphore.WaitAsync();
        }

        /// <inheritdoc />
        public void Release()
        {
            _semaphore.Release();
        }

        /// <inheritdoc />
        public bool ShouldIsolate(ISchema schema, GraphFieldSource fieldSource)
        {
            Validation.ThrowIfNull(schema, nameof(schema));

            return schema.Configuration.ExecutionOptions.DebugMode ||
                   schema.Configuration
                        .ExecutionOptions
                        .ResolverIsolation
                        .ShouldIsolateFieldSource(fieldSource);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _semaphore.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}