// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model
{
    /// <summary>
    /// A marker object used to represent no default value on an <see cref="IntrospectedInputValueType"/> type.
    /// This is used to differentiate "no default value" from a <c>~null~</c> default value.
    /// </summary>
    internal struct IntrospectionNoDefaultValue
    {
        /// <summary>
        /// Gets the singleton instance of this marker object.
        /// </summary>
        /// <value>The single instance of this object.</value>
        public static IntrospectionNoDefaultValue Instance { get; }
    }
}