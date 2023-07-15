// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;

    /// <summary>
    /// A declaration of the possible rules used by a schema to determine which
    /// arguments should be injected by a DI container and which should be part of the
    /// a query.
    /// </summary>
    public enum SchemaArgumentBindingRules
    {
        /// <summary>
        /// <para></para>
        /// Undecorated parameters will be treated as being part of the schema if the declared .NET type of the
        /// argument is part of the schema as an appropriate graph type (e.g. SCALAR, ENUM, INPUT_OBJECT).
        /// <para>
        /// Method parameters not included in the schema will attempt to be resolved from a scoped <see cref="IServiceProvider"/>
        /// when a field or directive resolver is invoked.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This is the default option for all schemas unless changed by the developer.
        /// </remarks>
        ParametersPreferQueryResolution = 0,

        /// <summary>
        /// <para>
        /// All method parameters intending to be included as arguments on the schema must be explicitly decorated using
        /// <c>[FromGraphQL]</c>.  Undecorated parameters will be treated as needing to be resolved
        /// from a scoped <see cref="IServiceProvider"/> when a field or directive resolver is invoked.
        /// </para>
        /// <para>
        /// Undecorated parameters WILL NOT be included as part of the schema.
        /// </para>
        /// </summary>
        ParametersRequireFromGraphQLDeclaration = 1,

        /// <summary>
        /// <para>
        /// All method parameters intending to be resolved from a scoped <see cref="IServiceProvider"/>
        /// must be explicitly declared using <c>[FromServices]</c>. Undecorated arguments will be treated as
        /// being part of the schema.
        /// </para>
        /// <para>
        /// Undecorated parameters WILL be included as part of the schema. This may lead to the schema being unable to be
        /// generated and the server failing to start.
        /// </para>
        /// </summary>
        ParametersRequireFromServicesDeclaration = 2,
    }
}