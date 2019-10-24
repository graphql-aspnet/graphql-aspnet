// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A represenstion of some data object with two properties, both declared as graph exposed items.
    /// </summary>
    [DebuggerDisplay("TwoProp: {Property1}")]
    public class TwoPropertyObject : ITwoPropertyObject
    {
        [GraphField]
        public string Property1 { get; set; }

        [GraphField]
        public int Property2 { get; set; }
    }
}