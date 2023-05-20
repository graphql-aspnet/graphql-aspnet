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
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Schemas;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A marker to a point in time that, when disposed, will reset the the global settings to the values
    /// that were present just before this object was created. Used in conjunction with NUnit to undo any changes to
    /// the global static providers in between tests.
    /// </summary>
    /// <remarks>
    /// If your test suite is configured to execute more than 1 test concurrently within the
    /// same app space this could cause unexpected results.
    /// </remarks>
    public class GraphQLGlobalRestorePoint : IDisposable
    {
        private readonly IGraphTypeTemplateProvider _templateProvider;
        private readonly IScalarGraphTypeProvider _scalarTypeProvider;
        private readonly IGraphTypeMakerProvider _makerProvider;
        private readonly ServiceLifetime _controllerServiceLifetime;
        private readonly int? _maxSubConnectedClient;
        private readonly int _maxSubConcurrentReceiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLGlobalRestorePoint" /> class.
        /// </summary>
        /// <param name="resetAllProviders">if set to <c>true</c> all providers will
        /// be immediately reset to their defualt implementation until this restore point is disposed.</param>
        public GraphQLGlobalRestorePoint(bool resetAllProviders = false)
        {
            _templateProvider = GraphQLProviders.TemplateProvider;
            _scalarTypeProvider = GraphQLProviders.ScalarProvider;
            _makerProvider = GraphQLProviders.GraphTypeMakerProvider;

            _controllerServiceLifetime = GraphQLServerSettings.ControllerServiceLifeTime;

            if (resetAllProviders)
            {
                GraphQLProviders.TemplateProvider = new DefaultTypeTemplateProvider();
                GraphQLProviders.ScalarProvider = new DefaultScalarGraphTypeProvider();
                GraphQLProviders.GraphTypeMakerProvider = new DefaultGraphTypeMakerProvider();
            }
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

                GraphQLServerSettings.ControllerServiceLifeTime = _controllerServiceLifetime;
            }
        }
    }
}