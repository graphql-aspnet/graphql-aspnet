// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface representing an established connection to a client that can send and receive
    /// messages from a server component.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor is built for.</typeparam>
    public interface ISubscriptionClientProxy<TSchema> : ISubscriptionClientProxy
        where TSchema : class, ISchema
    {
    }
}