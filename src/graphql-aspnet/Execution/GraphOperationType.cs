// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution
{
    /// <summary>
    /// A set of operation types supported by GraphQL.
    /// </summary>
    public enum GraphOperationType
    {
        Query = GraphCollection.Query,
        Mutation = GraphCollection.Mutation,
        Subscription = GraphCollection.Subscription,
    }
}