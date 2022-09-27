// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs
{
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A factory for generating instance of <see cref="ISubscriptionClientProxy{TSchema}"/>
    /// that supports the 'graphql-ws' protocol.
    /// </summary>
    internal class GqlwsLegacySubscriptionClientProxyFactoryAlternate : ApolloSubscriptionClientProxyFactory
    {
        /// <inheritdoc />
        public override string Protocol => ApolloConstants.ALTERNATE_PROTOCOL_NAME;
    }
}