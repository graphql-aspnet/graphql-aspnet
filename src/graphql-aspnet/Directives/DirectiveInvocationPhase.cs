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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An enumeration depicting the times when a directive might be invoked
    /// at runtime.
    /// </summary>
    [Flags]
    public enum DirectiveInvocationPhase
    {
        /// <summary>
        /// The current execution phase is unknown. This is representative of an error state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The directive is being invoked as part of the generation of a schema allowing
        /// the inspection and alteration of various generated <see cref="ISchemaItem"/> objects.
        /// </summary>
        SchemaGeneration = 1 << 0,

        /// <summary>
        /// The directive is being invoked during the execution of query document
        /// as part of a field resolution operation. This invocation is BEFORE the
        /// field of data is resolved.
        /// </summary>
        BeforeFieldResolution = 1 << 1,

        /// <summary>
        /// The directive is being invoked during the execution of query document
        /// as part of a field resolution operation. This invocation is AFTER the
        /// field of data is resolved.
        /// </summary>
        AfterFieldResolution = 1 << 2,
    }
}