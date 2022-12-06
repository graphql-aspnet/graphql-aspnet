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
    [DebuggerDisplay("TwoProp: {Property1}")]
    public class TwoPropertyObject : ITwoPropertyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwoPropertyObject"/> class.
        /// </summary>
        public TwoPropertyObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoPropertyObject"/> class.
        /// </summary>
        /// <param name="prop1">The value for <see cref="Property1"/>.</param>
        /// <param name="prop2">The value for <see cref="Property2"/>.</param>
        public TwoPropertyObject(string prop1, int prop2)
        {
            this.Property1 = prop1;
            this.Property2 = prop2;
        }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Property1}|{Property2}";
        }
    }
}