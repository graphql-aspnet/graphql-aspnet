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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An enumeration depicting when a directive might be invoked against a target item.
    /// </summary>
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
        SchemaGeneration = 1,

        /// <summary>
        /// The directive is being invoked during the execution of query document allowing
        /// the inspection and alteration of various <see cref="IDocumentPart"/> objects.
        /// </summary>
        QueryDocumentExecution = 2,
    }
}