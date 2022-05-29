// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    /// <summary>
    /// An enumeration of the valid operations of GraphQL.
    /// </summary>
    public enum GraphOperationType
    {
        Unknown = 0,
        Query = 1,
        Mutation = 2,
        Subscription = 3,
    }
}