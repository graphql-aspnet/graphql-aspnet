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
    /// <summary>
    /// A test object that has two properties of any type, both generic.
    /// </summary>
    /// <typeparam name="T1">The type of the first property.</typeparam>
    /// <typeparam name="T2">The type of the second property.</typeparam>
    public class TwoPropertyGenericObject<T1, T2>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T1 Property1 { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T2 Property2 { get; set; }
    }
}