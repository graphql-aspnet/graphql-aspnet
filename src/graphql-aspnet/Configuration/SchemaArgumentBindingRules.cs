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
    /// <summary>
    /// A declaration of the possible rules used by a schema to determine which
    /// arguments should be injected by a DI container and which should be part of the
    /// a query.
    /// </summary>
    public enum SchemaArgumentBindingRules
    {
        /// <summary>
        /// Undecorated arguments will be treated as being part of the schema if the declared .NET type of the
        /// argument is part of the schema. If the argument type is not part of the schema,
        /// the runtime will attempt to be resolved from the DI container when a query executes.
        /// </summary>
        ArgumentsPreferQueryResolution = 0,

        /// <summary>
        /// All arguments intended to be part of the schema must be explicitly decorated using
        /// <c>[FromGraphQL]</c>.  Undecorated arguments will be treated as needing to be resolved
        /// from a DI container when a query execute. Undecorated arguments WILL NOT be included as part of the schema.
        /// </summary>
        ArgumentsRequireGraphQlDeclaration = 1,

        /// <summary>
        /// All arguments intended to be resolved from the DI container during query execution must be explicitly
        /// declared using <c>[FromServices]</c>.
        /// Undecorated arguments will be treated as being resolved from a query and WILL be included
        /// as part of the schema.
        /// </summary>
        ArgumentsRequireDiDeclaration = 2,
    }
}