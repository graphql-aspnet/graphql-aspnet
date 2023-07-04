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
    /// <summary>
    /// A set of modifiers and flags that can be assigned to individual arguments on graph fields to modify their behavior
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

        // The Value 1 was deprecated and removed. Its value will not be re-used
        // to ensure no cross contamination of old code in referencing libraries.

        /// <summary>
        /// This parameter is declared to contain the result of the resolved parent field.
        /// </summary>
        ParentFieldResult = 2,

        /// <summary>
        /// This parameter is declared to be populated with the overall cancellation token
        /// governing the request or the default token if none was supplied on said request.
        /// </summary>
        CancellationToken = 4,

        /// <summary>
        /// This parameter is supplied to a field via a DI Container injection. It will not be exposed
        /// on the object graph.
        /// </summary>
        Injected = 8,
    }
}