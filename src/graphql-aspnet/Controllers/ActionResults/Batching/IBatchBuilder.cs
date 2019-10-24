// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Controllers.ActionResults.Batching
{
    /// <summary>
    /// A marker interface to identify a batchbuilder that may be returned from user code to throw a custom
    /// error message informing the developer of thier mistake.
    /// </summary>
    internal interface IBatchBuilder
    {
    }
}