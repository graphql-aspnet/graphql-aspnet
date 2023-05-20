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
    using GraphQL.AspNet.Tests.Framework.Interfaces;

    /// <summary>
    /// A set of providers used for configuring the behavior of the test framework.
    /// </summary>
    public static class GraphQLTestFrameworkProviders
    {
        private static IGraphQLTestAssertionHandler _handler = null;

        /// <summary>
        /// Gets or sets the failure handler used by the test framework when it
        /// must fail a test.
        /// </summary>
        /// <remarks>
        /// This property should be set as part of a global assembly setup before any
        /// tests are run.
        /// </remarks>
        /// <value>An assertion handler that can perform test assertions in your target
        /// unit testing library.</value>
        public static IGraphQLTestAssertionHandler Assertions
        {
            get
            {
                if (_handler == null)
                {
                    throw new InvalidOperationException(
                        $"The GraphQL Global Test Framework's {nameof(Assertions)} object " +
                        $"has not been initialized. Be sure to set '{nameof(GraphQLTestFrameworkProviders)}.{nameof(Assertions)}' " +
                        $"prior to invoking the components.");
                }

                return _handler;
            }

            set
            {
                _handler = value;
            }
        }
    }
}