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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An enumeration depicting the various collections of items tracked by graphql.
    /// </summary>
    public enum ItemPathRoots : int
    {
        // negative numbers represent internally defined roots
        // no mappable by developer code
        Introspection = -60,
        Schemas = -50,
        Directives = -40,
        Types = -10,
        Unknown = GraphOperationType.Unknown,
        Query = GraphOperationType.Query,
        Mutation = GraphOperationType.Mutation,
        Subscription = GraphOperationType.Subscription,
    }
}