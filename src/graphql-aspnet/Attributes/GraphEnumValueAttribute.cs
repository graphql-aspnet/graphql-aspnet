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
    /// When applied to an enum value, explicitly marks it as being included in a graph.
    /// Optionally, denotes its name in the graph as well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GraphEnumValueAttribute : GraphAttributeBase
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
        /// <value>The name given to this enum value.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a customized name to refer to this .NET enum value in all log entries and error messages.
        /// </summary>
        /// <remarks>
        /// When not supplied the name defaults to the qualified name of the enum value. (e.g. <c>'MyEnum.Value1'</c>).
        /// </remarks>
        /// <value>The name to refer to this enum value on internal messaging.</value>
        public string InternalName { get; set; }
    }
}