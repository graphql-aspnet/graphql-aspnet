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
    /// An enumeration depicting the various collections of items supported by
    /// graphql.
    /// </summary>
    public enum GraphCollection
    {
        Directives = -3,
        Enums = -2,
        Types = -1,
        Unknown = 0,
        Query = 1,
        Mutation = 2,
        Subscription = 3,
    }
}