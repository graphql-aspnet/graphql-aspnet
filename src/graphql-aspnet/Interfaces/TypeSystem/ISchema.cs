// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// A representation of a graphql schema that can have a query document executed against it and have a result generated.
    /// </summary>
    public interface ISchema
    {
        /// <summary>
        /// Gets the named operation types supported by this schema.
        /// </summary>
        /// <value>The root operations.</value>
        IDictionary<GraphCollection, IGraphOperation> OperationTypes { get; }

        /// <summary>
        /// Gets a collection of known graph object/scalar types available to this schema. Serves as the basis for the type
        /// system delivered on introspection requests as well as input value translation. <see cref="IGraphType"/> discovered
        /// as part of a <see cref="GraphController"/> registration are automatically included in this collection.
        /// </summary>
        /// <value>A collection of types, keyed on name, of the types registered and allowed on this schema.</value>
        ISchemaTypeCollection KnownTypes { get; }

        /// <summary>
        /// Gets the configuration options that governs the handling of this schema by graphql.
        /// </summary>
        /// <value>The configuration.</value>
        ISchemaConfiguration Configuration { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is fully setup and ready to use. This value is used to determine
        /// if the runtime needs to parse and add controller fields to this schema instance as part of a query execution.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        bool IsInitialized { get; set; }

        /// <summary>
        /// Gets the a common, friendly name for the schema. This name may appear in error messages.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}