// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    using System;

    /// <summary>
    /// Inspects the processor count available to this machine at regular intervals to help
    /// manage concurrency locks.
    /// Adapted from i3arnon's concurrent hash set: https://github.com/i3arnon/ConcurrentHashSet .
    /// </summary>
    internal static class ProcessorCounterInspector
    {
        private const int REFRESH_INTERVAL_MS = 30000;

        private static volatile int _processorCount;
        private static volatile int _lastProcessorCountRefreshTicks;

        /// <summary>
        /// Gets the count of current processors available to this machine at a given point in time.
        /// </summary>
        /// <value>The count.</value>
        internal static int Count
        {
            get
            {
                var now = Environment.TickCount;
                if (_processorCount == 0 || now - _lastProcessorCountRefreshTicks >= REFRESH_INTERVAL_MS)
                {
                    _processorCount = Environment.ProcessorCount;
                    _lastProcessorCountRefreshTicks = now;
                }

                return _processorCount;
            }
        }
    }
}