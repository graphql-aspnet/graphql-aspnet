// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable InconsistentNaming
#pragma warning disable SA1134 // Attributes should not share line
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// The possible locations a directive is defined for.
    /// </summary>
    [Flags]
    [GraphType(Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM)]
    [Description("An enumeration of the various declared locations within the schema or a query document.")]
    public enum DirectiveLocation
    {
        [GraphSkip]
        NONE = 0,

        // Execution Phase Locations
        QUERY = 1 << 0,
        MUTATION = 1 << 1,
        SUBSCRIPTION = 1 << 2,
        FIELD = 1 << 3,
        FRAGMENT_DEFINITION = 1 << 4,
        FRAGMENT_SPREAD = 1 << 5,
        INLINE_FRAGMENT = 1 << 6,

        // Type System Locations
        SCHEMA = 1 << 8,
        SCALAR = 1 << 9,
        OBJECT = 1 << 10,
        FIELD_DEFINITION = 1 << 11,
        ARGUMENT_DEFINITION = 1 << 12,
        INTERFACE = 1 << 13,
        UNION = 1 << 14,
        ENUM = 1 << 15,
        ENUM_VALUE = 1 << 16,
        INPUT_OBJECT = 1 << 17,
        INPUT_FIELD_DEFINITION = 1 << 18,

        /// <summary>
        /// All locations that target an executable query document.
        /// </summary>
        [GraphSkip]
        AllExecutionLocations = QUERY | MUTATION | SUBSCRIPTION |
            FIELD | FRAGMENT_DEFINITION | FRAGMENT_SPREAD |
            INLINE_FRAGMENT,

        /// <summary>
        /// All locations that target <see cref="ISchemaItem"/> instances.
        /// </summary>
        [GraphSkip]
        AllTypeDeclarationLocations = SCHEMA | SCALAR |
            OBJECT | FIELD_DEFINITION |
            ARGUMENT_DEFINITION | INTERFACE |
            UNION | ENUM | ENUM_VALUE | INPUT_OBJECT | INPUT_FIELD_DEFINITION,
    }
}