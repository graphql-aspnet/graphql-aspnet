// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.CommonHelpers
{
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.Interfaces;

    /// <summary>
    /// A represenstion of some data struct with two properties, both declared as graph exposed items.
    /// </summary>
    /// <remarks>
    /// This object can be used in unit tests as an OBJECT or INPUT_OBJECT graph type,
    /// The target of an INTERFACE graph type or as a member of a UNION graph type.
    /// </remarks>
    [DebuggerDisplay("TwoPropStruct: {Property1}")]
    public struct TwoPropertyStruct : ISinglePropertyObject
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        public string Property1 { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        public int Property2 { get; set; }
    }
}