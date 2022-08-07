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
    /// An enumeration depicting the various collections of items supported by
    /// graphql.
    /// </summary>
    public enum GraphCollection
    {
        // negative numbers represent internally defined collections
        // (not part of  the graph schema)
        Document = -70,
        Introspection = -60,
        Schemas = -50,
        Directives = -40,
        Scalars = -30,
        Enums = -20,
        Types = -10,
        Unknown = GraphOperationType.Unknown,
        Query = GraphOperationType.Query,
        Mutation = GraphOperationType.Mutation,
        Subscription = GraphOperationType.Subscription,
    }
}