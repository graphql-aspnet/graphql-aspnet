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
    public class IntrospectedEnumValue : IDeprecatable, INamedItem
    {
        private readonly IEnumOption _enumOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedEnumValue" /> class.
        /// </summary>
        /// <param name="enumOption">The enum option.</param>
        public IntrospectedEnumValue(IEnumOption enumOption)
        {
            _enumOption = Validation.ThrowIfNullOrReturn(enumOption, nameof(enumOption));
        }

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name => _enumOption.Name;

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description => _enumOption.Description;

        /// <summary>
        /// Gets a value indicating whether this item is depreciated. The <see cref="DeprecationReason" /> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated => _enumOption.IsDeprecated;

        /// <summary>
        /// Gets the provided reason for this item being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        public string DeprecationReason => _enumOption.DeprecationReason;
    }
}