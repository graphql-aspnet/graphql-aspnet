// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Model
{
    /// <summary>
    /// A marker class used to represent "no default value" on an __InputValue type. This is used to differentiate
    /// "no default value" from a "null" default value.
    /// </summary>
    internal class NoDefaultValue
    {
        /// <summary>
        /// Gets the singleton instance of this marker class.
        /// </summary>
        /// <value>The single instance of this class.</value>
        public static NoDefaultValue Instance { get; } = new NoDefaultValue();

        /// <summary>
        /// Prevents a default instance of the <see cref="NoDefaultValue"/> class from being created.
        /// </summary>
        private NoDefaultValue()
        {
        }
    }
}