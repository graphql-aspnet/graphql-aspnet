// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests
{
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Framework;

    /// <summary>
    /// A marker to a point in time that, when disposed, will reset the the global settings to the values
    /// that were present just before this object was created. Used in conjunction with NUnit to undo any changes to
    /// the global static providers in between tests.
    /// </summary>
    public class GraphQLGlobalSubscriptionRestorePoint : GraphQLGlobalRestorePoint
    {
        private readonly int? _maxSubConnectedClient;
        private readonly int _maxSubConcurrentReceiver;

        public GraphQLGlobalSubscriptionRestorePoint()
            : base()
        {
            _maxSubConnectedClient = GraphQLSubscriptionServerSettings.MaxConnectedClientCount;
            _maxSubConcurrentReceiver = GraphQLSubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount;

            SubscriptionEventSchemaMap.ClearCache();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                GraphQLSubscriptionServerSettings.MaxConnectedClientCount = _maxSubConnectedClient;
                GraphQLSubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount = _maxSubConcurrentReceiver;
            }
        }
    }
}