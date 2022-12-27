// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy
{
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A factory for generating instance of <see cref="ISubscriptionClientProxy{TSchema}"/>
    /// that supports the 'graphql-ws' protocol.
    /// </summary>
    internal class GraphqlWsLegacySubscriptionClientProxyFactoryAlternate : GraphqlWsLegacySubscriptionClientProxyFactory
    {
        /// <inheritdoc />
        public override string Protocol => GraphqlWsLegacyConstants.ALTERNATE_PROTOCOL_NAME;
    }
}