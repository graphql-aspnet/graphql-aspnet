// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

using GraphQL.AspNet.Interfaces.TypeSystem;

namespace GraphQL.Subscriptions.Tests.TestServerHelpers
{
    public interface IClientConnection<TSchema> where TSchema : class, ISchema
    {
    }
}