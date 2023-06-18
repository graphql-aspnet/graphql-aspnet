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
    /// A marker type used internally by the templating system to mark
    /// a dynamically built schema item that doesn't necessarily have a concrete
    /// matching type. This class cannot be inherited.
    /// </summary>
    internal sealed class RuntimeSchemaItemTypeMarker
    {
    }
}