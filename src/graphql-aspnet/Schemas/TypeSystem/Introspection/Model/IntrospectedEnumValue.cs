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
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A model object containing data for the __EnumValue type of one enum value in an <see cref="IEnumGraphType"/>.
    /// </summary>
    [GraphType(Constants.ReservedNames.ENUM_VALUE_TYPE)]
    [DebuggerDisplay("Introspected Enum Value: {Name}")]
    public sealed class IntrospectedEnumValue : IntrospectedItem, ISchemaItem
    {
        private readonly IEnumValue _enumValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedEnumValue" /> class.
        /// </summary>
        /// <param name="enumValue">The enum option.</param>
        public IntrospectedEnumValue(IEnumValue enumValue)
            : base(enumValue)
        {
            _enumValue = Validation.ThrowIfNullOrReturn(enumValue, nameof(enumValue));
        }

        /// <summary>
        /// Gets a value indicating whether the enum value this introspection item represents
        /// is deprecated.
        /// </summary>
        /// <value><c>true</c> if this instance is deprecated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated => _enumValue.IsDeprecated;

        /// <summary>
        /// Gets the reason, if any, why the enum vlaue this introspection item represents
        /// was deprecated.
        /// </summary>
        /// <value>The reason the target field was deprecated.</value>
        public string DeprecationReason => _enumValue.DeprecationReason;
    }
}