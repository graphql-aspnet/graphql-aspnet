// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal
{
    using System;

    /// <summary>
    /// A global, thread-safe counter to track total connected clients across schema instances and connection types.
    /// </summary>
    public sealed class GlobalConnectedSubscriptionClientCounter
    {
        private readonly object _locker = new object();
        private readonly int _maxConnectedClientCount;

        private int _currentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConnectedSubscriptionClientCounter"/> class.
        /// </summary>
        /// <param name="maxConnectedClientCount">The maximum connected client count.
        /// Once reached, this instance will deny increasing the count.</param>
        public GlobalConnectedSubscriptionClientCounter(int? maxConnectedClientCount = null)
        {
            _maxConnectedClientCount = maxConnectedClientCount ?? int.MaxValue;
            if (_maxConnectedClientCount <= 0)
                throw new ArgumentException("Max allowed concurrent client connections must be at least 1");

            _currentCount = 0;
        }

        /// <summary>
        /// Attempts to increase the count of connected clients by one.
        /// </summary>
        /// <returns><c>true</c> if the count was successfully increased, <c>false</c> otherwise.</returns>
        public bool IncreaseCount()
        {
            if (_currentCount >= _maxConnectedClientCount)
                return false;

            lock (_locker)
                _currentCount++;

            return true;
        }

        /// <summary>
        /// Decreases the count of connected clients by one.
        /// </summary>
        public void DecreaseCount()
        {
            lock (_locker)
            {
                if (_currentCount > 0)
                    _currentCount--;
            }
        }

        /// <summary>
        /// Gets the total number of connected clients.
        /// </summary>
        /// <value>The total connected clients.</value>
        public int TotalConnectedClients
        {
            get
            {
                lock (_locker)
                    return _currentCount;
            }
        }
    }
}