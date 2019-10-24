// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;

    /// <summary>
    /// An attribute that can be applied to an enum value to denote its name in the object graph. This
    /// name is subject to any name formatting rules for enumerations of the target schema.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GraphEnumValueAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphEnumValueAttribute"/> class.
        /// </summary>
        public GraphEnumValueAttribute()
            : this(Constants.Routing.ENUM_VALUE_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphEnumValueAttribute"/> class.
        /// </summary>
        /// <param name="name">The name to give this enumeration value.</param>
        public GraphEnumValueAttribute(string name)
        {
            this.Name = name?.Trim();
        }

        /// <summary>
        /// Gets the name to give this enumeration value.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }
    }
}