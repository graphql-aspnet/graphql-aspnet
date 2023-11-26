// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers
{
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A special controller instance for executing subscription runtime configured controller
    /// actions (e.g. minimal api defined fields).
    /// </summary>
    [GraphRoot]
    internal class SubscriptionEnabledRuntimeFieldExecutionController : GraphController
    {
    }
}