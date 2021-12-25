// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

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
    public enum ExecutableDirectiveLocation
    {
        [GraphSkip]
        NONE                = DirectiveLocation.NONE,

        QUERY               = DirectiveLocation.QUERY,
        MUTATION            = DirectiveLocation.MUTATION,
        SUBSCRIPTION        = DirectiveLocation.SUBSCRIPTION,
        FIELD               = DirectiveLocation.FIELD,
        FRAGMENT_DEFINITION = DirectiveLocation.FRAGMENT_DEFINITION,
        FRAGMENT_SPREAD     = DirectiveLocation.FRAGMENT_SPREAD,
        INLINE_FRAGMENT     = DirectiveLocation.INLINE_FRAGMENT,
        AllFieldSelections  = FIELD | FRAGMENT_SPREAD | INLINE_FRAGMENT,
    }
}