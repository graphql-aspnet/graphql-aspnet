// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A template describing a single value within an ENUM graph type.
    /// </summary>
    public class EnumValueTemplate : SchemaItemTemplateBase, IEnumValueTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValueTemplate" /> class.
        /// </summary>
        /// <param name="parentTemplate">The parent template.</param>
        /// <param name="enumFieldInfo">The enum field information.</param>
        public EnumValueTemplate(IEnumGraphTypeTemplate parentTemplate, FieldInfo enumFieldInfo)
            : base(enumFieldInfo)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parentTemplate, nameof(parentTemplate));
            this.FieldInfo = Validation.ThrowIfNullOrReturn(enumFieldInfo, nameof(enumFieldInfo));
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            this.ObjectType = this.Parent.ObjectType;

            var parentTypeIsUnSigned = this.Parent.ObjectType.IsEnumOfUnsignedNumericType();
            this.Value = this.FieldInfo.GetValue(null);

            this.NumericValueAsString = parentTypeIsUnSigned
                ? Convert.ToUInt64(this.Value).ToString()
                : Convert.ToInt64(this.Value).ToString();

            this.Description = this.FieldInfo.SingleAttributeOrDefault<DescriptionAttribute>()?.Description ?? null;
            var enumAttrib = this.FieldInfo.SingleAttributeOrDefault<GraphEnumValueAttribute>();

            var valueName = enumAttrib?.Name?.Trim() ?? Constants.Routing.ENUM_VALUE_META_NAME;
            if (valueName.Length == 0)
                valueName = Constants.Routing.ENUM_VALUE_META_NAME;

            valueName = valueName.Replace(Constants.Routing.ENUM_VALUE_META_NAME, this.FieldInfo.Name);
            this.Route = new SchemaItemPath(SchemaItemPath.Join(this.Parent.Route.Path, valueName));
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            GraphValidation.EnsureGraphNameOrThrow($"{this.InternalFullName}", this.Name);
        }

        /// <inheritdoc />
        public override Type ObjectType => this.Parent.ObjectType;

        /// <inheritdoc />
        public IEnumGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the raw field information that describes this enum value.
        /// </summary>
        /// <value>The field information.</value>
        public FieldInfo FieldInfo { get; }

        /// <inheritdoc />
        public object Value { get; private set; }

        /// <inheritdoc />
        public string NumericValueAsString { get; private set; }

        /// <inheritdoc />
        public override string InternalFullName => $"{this.Parent.InternalFullName}.{this.InternalName}";

        /// <inheritdoc />
        public override string InternalName => this.FieldInfo.Name;
    }
}