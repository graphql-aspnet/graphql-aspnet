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
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// The possible locations a directive can be applied within a schema or query document.
    /// </summary>
    [Flags]
    [GraphType(Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM)]
    [Description("An enumeration of the named locations within a schema or a query document.")]
    public enum DirectiveLocation
    {
        [GraphSkip]
        NONE = 0,

        // Execution Phase Locations
        QUERY                       = 1 << 0,
        MUTATION                    = 1 << 1,
        SUBSCRIPTION                = 1 << 2,
        FIELD                       = 1 << 3,
        FRAGMENT_DEFINITION         = 1 << 4,
        FRAGMENT_SPREAD             = 1 << 5,
        INLINE_FRAGMENT             = 1 << 6,
        VARIABLE_DEFINITION         = 1 << 7,

        // Type System Locations
        SCHEMA                      = 1 << 8,
        SCALAR                      = 1 << 9,
        OBJECT                      = 1 << 10,
        FIELD_DEFINITION            = 1 << 11,
        ARGUMENT_DEFINITION         = 1 << 12,
        INTERFACE                   = 1 << 13,
        UNION                       = 1 << 14,
        ENUM                        = 1 << 15,
        ENUM_VALUE                  = 1 << 16,
        INPUT_OBJECT                = 1 << 17,
        INPUT_FIELD_DEFINITION      = 1 << 18,

        /// <summary>
        /// All locations that target an <see cref="IDocumentPart"/>.
        /// </summary>
        [GraphSkip]
        AllExecutionLocations =
              QUERY
            | MUTATION
            | SUBSCRIPTION
            | FIELD
            | FRAGMENT_DEFINITION
            | FRAGMENT_SPREAD
            | INLINE_FRAGMENT
            | VARIABLE_DEFINITION,

        /// <summary>
        /// All locations that target <see cref="ISchemaItem"/> instances.
        /// </summary>
        [GraphSkip]
        AllTypeSystemLocations =
              SCHEMA
            | SCALAR
            | OBJECT
            | FIELD_DEFINITION
            | ARGUMENT_DEFINITION
            | INTERFACE
            | UNION
            | ENUM
            | ENUM_VALUE
            | INPUT_OBJECT
            | INPUT_FIELD_DEFINITION,
    }
}