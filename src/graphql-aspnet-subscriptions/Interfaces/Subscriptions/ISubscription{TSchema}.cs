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
    /// A client subscription containing the details of what event is being listened for.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema the subscription exists for.</typeparam>
    public interface ISubscription<TSchema> : ISubscription
        where TSchema : class, ISchema
    {
    }
}