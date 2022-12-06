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
    using System;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A representation of a data object with two properties. The property data types differ from <see cref="TwoPropertyObject"/>.
    /// </summary>
    public class TwoPropertyObjectV3 : ITwoPropertyObject
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
        public DateTime Property2 { get; set; }
    }
}