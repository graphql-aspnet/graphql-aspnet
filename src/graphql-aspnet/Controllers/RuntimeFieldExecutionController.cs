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
    /// A special controller instance for executing runtime configured controller
    /// actions (e.g. minimal api defined fields and type exensions). This class can only be
    /// instantiated by the library runtime.
    /// </summary>
    [GraphRoot]
    internal sealed class RuntimeFieldExecutionController : GraphController
    {
    }
}