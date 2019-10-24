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
    /// <summary>
    /// An enumeration representing where a source field was parsed from.
    /// </summary>
    public enum GraphFieldTemplateSource
    {
        None,
        Action,
        Method,
        Property,
    }
}