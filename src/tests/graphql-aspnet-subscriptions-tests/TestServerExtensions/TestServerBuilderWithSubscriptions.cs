// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.TestServerExtensions
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    public class TestServerBuilderWithSubscriptions<TSchema>
        where TSchema : class, ISchema
    {
        // populate with helper methods for boiler plating the subscription server
        // with apollo as a wrapper for the test server itself.
    }
}