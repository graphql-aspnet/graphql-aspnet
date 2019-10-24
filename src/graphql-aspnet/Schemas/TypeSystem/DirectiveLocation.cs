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
        NONE = 0,
        QUERY = 1,
        MUTATION = 2,
        SUBSCRIPTION = 4,
        FIELD = 8,
        FRAGMENT_DEFINITION = 16,
        FRAGMENT_SPREAD = 32,
        INLINE_FRAGMENT = 64,
        SCHEMA = 128,
        SCALAR = 256,
        OBJECT = 512,
        FIELD_DEFINITION = 1024,
        ARGUMENT_DEFINITION = 2048,
        INTERFACE = 4096,
        UNION = 8192,
        ENUM = 16384,
        ENUM_VALUE = 32768,
        INPUT_OBJECT = 65536,
        INPUT_FIELD_DEFINITION = 131072,
    }
}