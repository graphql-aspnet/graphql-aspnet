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
    /// An enumeration denoting the severity of a message generated during the
    /// completion of a graph operation.
    /// </summary>
    public enum GraphMessageSeverity
    {
        Trace = 0,
        Debug = 25,
        Information = 50,
        Warning = 75,
        Critical = 100,
    }
}