// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole
{
    /// <summary>
    /// A set of constants used by this app.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The full url to the graphql end point.
        /// </summary>
        public const string GRAPHQL_URL = "http://localhost:3000/graphql";

        /// <summary>
        /// The url stem pointing to where rest requests should be set.
        /// </summary>
        public const string REST_URL_BASE = "http://localhost:3000";
    }
}