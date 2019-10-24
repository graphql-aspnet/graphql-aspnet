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
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// The various kinds of graph types supported by this server.
    /// </summary>
    [GraphType(Constants.ReservedNames.TYPE_KIND_ENUM)]
    [Description("An enumeration of the possible graph types this server declares.")]
    public enum TypeKind
    {
        [GraphSkip]
        NONE,

        SCALAR,
        OBJECT,
        INTERFACE,
        UNION,
        ENUM,
        INPUT_OBJECT,
        LIST,
        NON_NULL,

        [GraphSkip]
        DIRECTIVE,
    }
}