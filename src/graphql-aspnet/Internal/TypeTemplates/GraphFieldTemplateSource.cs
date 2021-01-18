// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;

    /// <summary>
    /// An enumeration representing where a source field was parsed from.
    /// </summary>
    [Flags]
    public enum GraphFieldTemplateSource
    {
        None = 0,
        Action = 1,
        Method = 2,
        Property = 4,
    }
}