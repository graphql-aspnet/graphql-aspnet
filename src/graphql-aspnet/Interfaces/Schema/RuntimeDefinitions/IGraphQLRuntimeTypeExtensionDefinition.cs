// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration.Templates
{
    using System;
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// An intermediate template that utilizies a key/value pair system to build up a set of component parts
    /// that the templating engine will use to generate a full fledged type extension in a schema.
    /// </summary>
    public interface IGraphQLRuntimeTypeExtensionDefinition : IGraphQLRuntimeSchemaItemDefinition, IGraphQLResolvableSchemaItemDefinition
    {
        /// <summary>
        /// Gets the concrcete type of the OBJECT or INTERFACE that will be extended.
        /// </summary>
        /// <value>The class, interface or struct that will be extended with this new field.</value>
        Type TargetType { get; }

        /// <summary>
        /// Gets or sets the expected processing mode of data when this field is invoked
        /// by the runtime.
        /// </summary>
        /// <value>The execution mode of this type extension.</value>
        FieldResolutionMode ExecutionMode { get; set; }
    }
}