// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Directives
{
    /// <summary>
    /// An enumeration dictating where, in the execution of the item adjacent to the directive,
    /// the directive method should execute.
    /// </summary>
    public enum DirectiveLifeCycle
    {
        BeforeResolution,
        AfterResolution,
    }
}