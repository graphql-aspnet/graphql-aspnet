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
    public enum GraphFieldSource
    {
        /// <summary>
        /// Unknown field source.
        /// </summary>
        None = 0,

        /// <summary>
        /// The field originated from a controller action.
        /// </summary>
        Action = 1,

        /// <summary>
        /// The field originated from a general object method.
        /// </summary>
        Method = 2,

        /// <summary>
        /// The field originated from a general object property.
        /// </summary>
        Property = 4,

        /// <summary>
        /// The field originated from an internal, virtual object that faciliates routing.
        /// </summary>
        Virtual = 8,
    }
}