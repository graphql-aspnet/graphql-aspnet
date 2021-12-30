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
    public class IntrospectedEnumValue : IDeprecatable, ISchemaItem
    {
        private readonly IEnumOption _enumOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedEnumValue" /> class.
        /// </summary>
        /// <param name="enumOption">The enum option.</param>
        public IntrospectedEnumValue(IEnumOption enumOption)
        {
            _enumOption = Validation.ThrowIfNullOrReturn(enumOption, nameof(enumOption));
            this.AppliedDirectives = new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public string Name => _enumOption.Name;

        /// <inheritdoc />
        public string Description => _enumOption.Description;

        /// <inheritdoc />
        public bool IsDeprecated => _enumOption.IsDeprecated;

        /// <inheritdoc />
        public string DeprecationReason => _enumOption.DeprecationReason;

        /// <inheritdoc />
        [GraphSkip]
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}