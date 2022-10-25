// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A marker to a point in time that, when disposed, will reset the the global settings to the values
    /// that were present just before this object was created. Used in conjunction with NUnit to undo any changes to
    /// the global static providers in between tests.
    /// </summary>
    public class GraphQLGlobalRestorePoint : IDisposable
    {
        private readonly IGraphTypeTemplateProvider _templateProvider;
        private readonly IScalarTypeProvider _scalarTypeProvider;
        private readonly IGraphTypeMakerProvider _makerProvider;
        private readonly IGraphQLGlobalConfiguration _globalConfig;
        private readonly int? _maxSubConnectedClient;
        private readonly int _maxSubConcurrentReceiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLGlobalRestorePoint"/> class.
        /// </summary>
        public GraphQLGlobalRestorePoint()
        {
            _templateProvider = GraphQLProviders.TemplateProvider;
            _scalarTypeProvider = GraphQLProviders.ScalarProvider;
            _makerProvider = GraphQLProviders.GraphTypeMakerProvider;
            _globalConfig = GraphQLProviders.GlobalConfiguration;

            _maxSubConnectedClient = SubscriptionServerSettings.MaxConnectedClientCount;
            _maxSubConcurrentReceiver = SubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount;

            SchemaSubscriptionEventMap.ClearCache();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GraphQLProviders.TemplateProvider = _templateProvider;
                GraphQLProviders.ScalarProvider = _scalarTypeProvider;
                GraphQLProviders.GraphTypeMakerProvider = _makerProvider;
                GraphQLProviders.GlobalConfiguration = _globalConfig;

                SubscriptionServerSettings.MaxConnectedClientCount = _maxSubConnectedClient;
                SubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount = _maxSubConcurrentReceiver;
            }
        }
    }
}