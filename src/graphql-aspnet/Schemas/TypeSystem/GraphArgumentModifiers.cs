// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;

    /// <summary>
    /// A set of modifiers that can be assigned to individual arguments on graph fields to modify their behavior
    /// during execution.
    /// </summary>
    public enum GraphArgumentModifiers
    {
        // implementation note, this used to be a [Flags] enum
        // kept numbering of previous usage to prevent clashing in other libraries.

        /// <summary>
        /// No special modifications are needed.
        /// </summary>
        None = 0,

        /// <summary>
        /// This parameter is declared to contain the result of the value returned from the parent field's resolver. Applicable to
        /// type extension and batch extension action methods.
        /// </summary>
        ParentFieldResult,

        /// <summary>
        /// This parameter is declared to be a cancellation token
        /// governing the request or the default token if none was supplied on said request.
        /// </summary>
        CancellationToken,

        /// <summary>
        /// This parameter is declared to be resolved via dependency injection. It will NEVER be exposed
        /// on the object graph. If the type represented by this parameter is not servable via a scoped <see cref="IServiceProvider"/>
        /// instance, an exception will occur and the target query will not be resolved.
        /// </summary>
        ExplicitInjected,

        /// <summary>
        /// This parameter is declared to be resolved as an argument to a graph field. It will ALWAYS be
        /// exposed on the object graph. If the type represented by this parameter cannot be served from the object graph
        /// an exception will occur and the schema will fail to generate. This can occur, for instance, if it is an interface,
        /// or if the type was explicitly excluded from the graph via attributions.
        /// </summary>
        ExplicitSchemaItem,
    }
}