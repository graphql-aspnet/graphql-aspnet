// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    /// <summary>
    /// An enum that indicates where a piece of data (typically a graph type or resolver) was templated from.
    /// </summary>
    public enum ItemSource
    {
        /// <summary>
        /// Indicates that the data item was created from code declared at design time. That it was a
        /// defined, precomipled type in the developer's source code.
        /// </summary>
        DesignTime,

        /// <summary>
        /// Indicates that the data item was created from code declared at run time. That it was configured
        /// at startup, after the program code had been compiled.
        /// </summary>
        Runtime,
    }
}