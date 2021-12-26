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
    using System;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// An enumeration dictating where this directive may appear in the lifecycle of a graph schema.
    /// </summary>
    [Flags]
    [GraphSkip]
    public enum DirectiveLifeCyclePhase
    {
        /// <summary>
        /// An unknown phase, indicitive of an error state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The directive should be invoked before field resolution during the execution phase
        /// of a query document.
        /// </summary>
        BeforeResolution = 1 << 0,

        /// <summary>
        /// The directive should be invoked after field resolution during the execution phase
        /// of a query document.
        /// </summary>
        AfterResolution  = 1 << 1,

        /// <summary>
        /// The directive should be invoked during the building of a schema's type system.
        /// </summary>
        AlterTypeSystem  = 1 << 2,
    }
}