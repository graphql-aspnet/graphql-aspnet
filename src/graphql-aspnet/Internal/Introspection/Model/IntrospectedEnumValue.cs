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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A model object containing data for the __EnumValue type of one enum value in an <see cref="IEnumGraphType"/>.
    /// </summary>
    [GraphType(Constants.ReservedNames.ENUM_VALUE_TYPE)]
    [DebuggerDisplay("Introspected Enum Value: {Name}")]
    public class IntrospectedEnumValue : IntrospectedItem, IDeprecatable, ISchemaItem
    {
        private readonly IEnumValue _enumOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedEnumValue" /> class.
        /// </summary>
        /// <param name="enumOption">The enum option.</param>
        public IntrospectedEnumValue(IEnumValue enumOption)
            : base(enumOption)
        {
            _enumOption = Validation.ThrowIfNullOrReturn(enumOption, nameof(enumOption));
            this.IsDeprecated = _enumOption.IsDeprecated;
            this.DeprecationReason = _enumOption.DeprecationReason;
        }

        /// <inheritdoc />
        public bool IsDeprecated { get; set; }

        /// <inheritdoc />
        public string DeprecationReason { get; set; }
    }
}