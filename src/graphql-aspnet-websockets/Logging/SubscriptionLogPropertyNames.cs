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
    }
}