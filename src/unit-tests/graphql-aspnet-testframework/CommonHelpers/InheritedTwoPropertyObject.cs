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

    /// <summary>
    /// A represenstion of some data object with two properties, both declared as graph exposed items.
    /// </summary>
    [DebuggerDisplay("InheritedTwoProp: {Property1}")]
    public class InheritedTwoPropertyObject : TwoPropertyObject
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>value.</value>
        [GraphField]
        public long Property3 { get; set; }
    }
}