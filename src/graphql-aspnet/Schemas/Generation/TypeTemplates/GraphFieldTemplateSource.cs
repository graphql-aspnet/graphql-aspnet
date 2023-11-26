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
    using System;

    /// <summary>
    /// An enumeration representing where a given field or field template was sourced from.
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