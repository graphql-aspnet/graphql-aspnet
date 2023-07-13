// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions
{
    using System;

    /// <summary>
    /// A template for a runtime created schema item that has an attached resolver. Usually
    /// a field or a directive.
    /// </summary>
    public interface IGraphQLResolvableSchemaItemDefinition : IGraphQLRuntimeSchemaItemDefinition
    {
        /// <summary>
        /// Gets or sets the resolver function that has been assigned to execute when this
        /// schema item is requested or processed.
        /// </summary>
        /// <value>The field's assigned resolver.</value>
        Delegate Resolver { get; set; }

        /// <summary>
        /// Gets or sets the explicitly declared return type of this schema item. Can be
        /// null if the <see cref="Resolver"/> returns a valid concrete type.
        /// </summary>
        /// <value>The data type this schema item will return.</value>
        Type ReturnType { get; set; }
    }
}