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
    /// A represenstion of some data object with three properties, two
    /// inherited from a base type and a third explictly declared within the class.
    /// </summary>
    /// <remarks>
    /// This object can be used in unit tests as an OBJECT or INPUT_OBJECT graph type,
    /// The target of an INTERFACE graph type or as a member of a UNION graph type.
    /// </remarks>
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