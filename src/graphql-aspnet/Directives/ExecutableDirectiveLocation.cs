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
    /// which indicate a document location that can be acted on.
    /// </summary>
    [Flags]
    public enum ExecutableDirectiveLocation
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
        AllFieldSelections = FIELD | FRAGMENT_SPREAD | INLINE_FRAGMENT,
    }
}