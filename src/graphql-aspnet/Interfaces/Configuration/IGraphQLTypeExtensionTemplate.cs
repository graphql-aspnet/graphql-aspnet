// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System;
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// An intermediate template that utilizies a key/value pair system to build up a set of component parts
    /// that the templating engine will use to generate a full fledged type extension in a schema.
    /// </summary>
    public interface IGraphQLTypeExtensionTemplate : IGraphQLResolvedFieldTemplate
    {
        /// <summary>
        /// Gets the execution mode of this type extension.
        /// </summary>
        /// <remarks>
        /// The mode must match the resolver definition for this
        /// instance or an exception will be thrown.
        /// </remarks>
        /// <value>The execution mode of this type extension.</value>
        public FieldResolutionMode ExecutionMode { get; }

        /// <summary>
        /// Gets the OBJECT or INTERFACE type that will be extended.
        /// </summary>
        /// <value>The class, interface or struct that will be extended with this new field.</value>
        public Type TargetType { get; }
    }
}