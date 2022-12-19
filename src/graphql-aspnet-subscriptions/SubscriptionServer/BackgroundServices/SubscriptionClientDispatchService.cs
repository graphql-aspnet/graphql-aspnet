// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.BackgroundServices
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Internal;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// A background service that throttles the flow of recieved events
    /// to the various clients that should recieve them.
    /// </summary>
    internal sealed class SubscriptionClientDispatchService : BackgroundService
    {
        private readonly ISubscriptionEventDispatchQueue _dispatchQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientDispatchService"/> class.
        /// </summary>
        /// <param name="dispatchQueue">The dispatch queue
        /// this service is responsible for.</param>
        public SubscriptionClientDispatchService(ISubscriptionEventDispatchQueue dispatchQueue)
        {
            _dispatchQueue = Validation.ThrowIfNullOrReturn(dispatchQueue, nameof(dispatchQueue));
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _dispatchQueue.BeginProcessingQueueAsync(stoppingToken);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _dispatchQueue.StopQueue();
            _dispatchQueue.Dispose();
            base.Dispose();
        }
    }
}