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
    /// A set of modifiers and flags that can be assigned to individual arguments on graph fields to modify their behavior
    /// during execution.
    /// </summary>
    /// <remarks>
    /// This enumeration was originally intended to be a bitwise flag but was changed with v2.0 was changed to be
    /// a set of distinct values. The power of 2 increments was preserved for backwards compatiability.
    /// </remarks>
    public enum GraphArgumentModifiers
    {
        // implementation note, this used to be a [Flags] enum
        // kept numbering of previous usage to prevent clashing in other libraries.

        /// <summary>
        /// No special modifications are needed.
        /// </summary>
        None = 0,

        // The Value '1' was deprecated and removed. Its value will not be re-used
        // to ensure no cross contamination of old code in referencing libraries.

        /// <summary>
        /// This parameter is declared to contain the result of the value returned from the parent field's resolver. Applicable to
        /// type extension and batch extension action methods.
        /// </summary>
        ParentFieldResult = 2,

        /// <summary>
        /// This parameter is declared to be a cancellation token
        /// governing the request or the default token if none was supplied on said request.
        /// </summary>
        CancellationToken = 4,

        /// <summary>
        /// This parameter is declared to be resolved via dependency injection. It will NEVER be exposed
        /// on the object graph. If the type represented by this parameter is not servable via a scoped <see cref="IServiceProvider"/>
        /// instance, an exception will occur and the target query will not be resolved.
        /// </summary>
        ExplicitInjected = 8,

        /// <summary>
        /// This parameter is declared to be resolved as an argument to a graph field. It will ALWAYS be
        /// exposed on the object graph. If the type represented by this parameter cannot be served from the object graph
        /// an exception will occur and the schema will fail to generate. This can occur, for instance, if it is an interface,
        /// or if the type was explicitly excluded from the graph via attributions.
        /// </summary>
        ExplicitSchemaItem = 16,
    }
}