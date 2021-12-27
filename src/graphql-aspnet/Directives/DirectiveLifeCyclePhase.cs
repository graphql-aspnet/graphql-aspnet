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
    using System.ComponentModel;

    /// <summary>
    /// An enumeration detailing the various directive lifecycle phases.
    /// </summary>
    internal enum DirectiveLifeCyclePhase
    {
        Unknown = 0,

        [Description("Schema Construction")]
        SchemaBuilding = 1 << 0,

        [Description("Query Execution")]
        Execution = 1 << 1,
    }
}