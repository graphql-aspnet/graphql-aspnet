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
    using DLCEA = GraphQL.AspNet.Attributes.DirectiveLifeCycleEventAttribute;
    using DLE = GraphQL.AspNet.Directives.DirectiveLifeCycleEvent;

    /// <summary>
    /// The possible locations a directive is defined for.
    /// </summary>
    [Flags]
    [GraphType(Constants.ReservedNames.DIRECTIVE_LOCATION_ENUM)]
    [Description("An enumeration of the various declared locations within the schema or a query document.")]
    public enum DirectiveLocation
    {
        [GraphSkip] NONE = 0,

        // Execution Phase Locations
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] QUERY                   = 1 << 0,
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] MUTATION                = 1 << 1,
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] SUBSCRIPTION            = 1 << 2,
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] FIELD                   = 1 << 3,
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] FRAGMENT_DEFINITION     = 1 << 4,
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] FRAGMENT_SPREAD         = 1 << 5,
        [DLCEA(DLE.BeforeResolution | DLE.AfterResolution)] INLINE_FRAGMENT         = 1 << 6,

        // Type System Locations
        [DLCEA(DLE.AlterTypeSystem)] SCHEMA                  = 1 << 7,
        [DLCEA(DLE.AlterTypeSystem)] SCALAR                  = 1 << 8,
        [DLCEA(DLE.AlterTypeSystem)] OBJECT                  = 1 << 9,
        [DLCEA(DLE.AlterTypeSystem)] FIELD_DEFINITION        = 1 << 10,
        [DLCEA(DLE.AlterTypeSystem)] ARGUMENT_DEFINITION     = 1 << 11,
        [DLCEA(DLE.AlterTypeSystem)] INTERFACE               = 1 << 12,
        [DLCEA(DLE.AlterTypeSystem)] UNION                   = 1 << 13,
        [DLCEA(DLE.AlterTypeSystem)] ENUM                    = 1 << 14,
        [DLCEA(DLE.AlterTypeSystem)] ENUM_VALUE              = 1 << 15,
        [DLCEA(DLE.AlterTypeSystem)] INPUT_OBJECT            = 1 << 16,
        [DLCEA(DLE.AlterTypeSystem)] INPUT_FIELD_DEFINITION  = 1 << 17,
    }
}