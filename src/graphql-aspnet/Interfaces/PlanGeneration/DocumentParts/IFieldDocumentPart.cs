// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A single field of data from a source object selected to be returned as part of a graph query.
    /// </summary>
    public interface IFieldDocumentPart : ISecureDocumentPart, IDirectiveContainerDocumentPart, IIncludeableDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Gets or sets a function that, when supplied, will be called immediately after
        /// this field is resolved for a given source object.
        /// </summary>
        /// <value>The post processor.</value>
        Func<FieldResolutionContext, CancellationToken, Task> PostProcessor { get; set; }

        /// <summary>
        /// Gets the name of the field requested, as it exists in the schema.
        /// </summary>
        /// <value>The parsed name from the queryt document.</value>
        ReadOnlyMemory<char> Name { get; }

        /// <summary>
        /// Gets the alias assigned to the field requested as it was defined in the user's query document.
        /// Defaults to <see cref="Name"/> if not supplied.
        /// </summary>
        /// <value>The parsed alias from the query document.</value>
        ReadOnlyMemory<char> Alias { get; }

        /// <summary>
        /// Gets the field reference pointed to by this instance as its declared in the schema.
        /// </summary>
        /// <value>The field.</value>
        IGraphField Field { get; }

        /// <summary>
        /// Gets the field selection set, if any, contained in this field.
        /// </summary>
        /// <value>The child selection set.</value>
        IFieldSelectionSetDocumentPart FieldSelectionSet { get; }

        /// <summary>
        /// Gets the arguments defined on this field.
        /// </summary>
        /// <value>The arguments.</value>
        IInputArgumentCollectionDocumentPart Arguments { get; }
    }
}