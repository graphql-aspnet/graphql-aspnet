// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable InconsistentNaming
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// The possible locations a directive is defined for.
    /// </summary>
    [Flags]
    [GraphType(Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM)]
    [Description("An enumeration of the various declared locations within the schema or a query document.")]
    public enum DirectiveLocation
    {
        [GraphSkip]
        NONE                    = 0,

        QUERY                   = 1 << 0,
        MUTATION                = 1 << 1,
        SUBSCRIPTION            = 1 << 2,
        FIELD                   = 1 << 3,
        FRAGMENT_DEFINITION     = 1 << 4,
        FRAGMENT_SPREAD         = 1 << 5,
        INLINE_FRAGMENT         = 1 << 6,
        SCHEMA                  = 1 << 7,
        SCALAR                  = 1 << 8,
        OBJECT                  = 1 << 9,
        FIELD_DEFINITION        = 1 << 10,
        ARGUMENT_DEFINITION     = 1 << 11,
        INTERFACE               = 1 << 12,
        UNION                   = 1 << 13,
        ENUM                    = 1 << 14,
        ENUM_VALUE              = 1 << 15,
        INPUT_OBJECT            = 1 << 16,
        INPUT_FIELD_DEFINITION  = 1 << 17,
    }
}