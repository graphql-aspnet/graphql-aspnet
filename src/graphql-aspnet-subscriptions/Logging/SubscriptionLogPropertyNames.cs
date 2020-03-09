// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Logging
{
    /// <summary>
    /// A set of property names used by subscription log events.
    /// </summary>
    public static class SubscriptionLogPropertyNames
    {
        /// <summary>
        /// The subscription route assigned to a schema type when it was registered with ASP.NET.
        /// </summary>
        public const string SCHEMA_SUBSCRIPTION_ROUTE_PATH = "subscriptionRoute";

        /// <summary>
        /// The unique id assigned to the subscription client when it was first created
        /// by the configured subscription server.
        /// </summary>
        public const string SUBSCRIPTION_CLIENT_ID = "clientId";

        /// <summary>
        /// The type name of the client that was instantiated by the subscription server.
        /// </summary>
        public const string SUBSCRIPTION_CLIENT_TYPE_NAME = "clientType";

        /// <summary>
        /// The type name of the server that was involved in the transaction.
        /// </summary>
        public const string SUBSCRIPTION_SERVER_TYPE_NAME = "serverType";
    }
}