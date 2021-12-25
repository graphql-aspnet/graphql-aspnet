// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable InconsistentNaming
namespace GraphQL.AspNet.Directives
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A subset of <see cref="DirectiveLocation"/> identifying those locations
    /// which indicate an inbound query document location that can be acted on.
    /// </summary>
    [Flags]
    public enum TypeSystemDirectiveLocation
    {
        [GraphSkip]
        NONE                        = DirectiveLocation.NONE,

        SCHEMA                      = DirectiveLocation.SCHEMA,
        SCALAR                      = DirectiveLocation.SCALAR,
        OBJECT                      = DirectiveLocation.OBJECT,
        FIELD_DEFINITION            = DirectiveLocation.FIELD_DEFINITION,
        ARGUMENT_DEFINITION         = DirectiveLocation.ARGUMENT_DEFINITION,
        INTERFACE                   = DirectiveLocation.INTERFACE,
        UNION                       = DirectiveLocation.UNION,
        ENUM                        = DirectiveLocation.ENUM,
        ENUM_VALUE                  = DirectiveLocation.ENUM_VALUE,
        INPUT_OBJECT                = DirectiveLocation.INPUT_OBJECT,
        INPUT_FIELD_DEFINITION      = DirectiveLocation.INPUT_FIELD_DEFINITION,
    }
}