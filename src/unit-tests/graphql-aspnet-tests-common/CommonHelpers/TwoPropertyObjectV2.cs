// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.CommonHelpers
{
    using System;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A representation of a data object with two properties. The property data types differ from <see cref="TwoPropertyObject"/>
    /// for additional value type testing.
    /// </summary>
    /// <remarks>
    /// This object can be used in unit tests as an OBJECT or INPUT_OBJECT graph type,
    /// The target of an INTERFACE graph type or as a member of a UNION graph type.
    /// </remarks>
    public class TwoPropertyObjectV2
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        public float Property1 { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        public DateTime Property2 { get; set; }
    }
}