namespace GraphQL.AspNet.Tests.Framework.CommonHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A test object that has two properties, both generic.
    /// </summary>
    /// <typeparam name="T1">The type of the first prop.</typeparam>
    /// <typeparam name="T2">The type of the second prop.</typeparam>
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
