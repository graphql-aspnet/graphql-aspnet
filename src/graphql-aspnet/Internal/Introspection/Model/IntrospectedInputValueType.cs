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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of data about a graph type being exposed as an input to another field.
    /// </summary>
    [DebuggerDisplay("Introspected Input Value: {Name}")]
    public class IntrospectedInputValueType : IntrospectedItem, ISchemaItem
    {
        private readonly object _rawDefaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedInputValueType" /> class.
        /// </summary>
        /// <param name="argument">The argument being introspected.</param>
        /// <param name="introspectedGraphType">The meta data representing the type of this argument.</param>
        public IntrospectedInputValueType(IGraphArgument argument, IntrospectedType introspectedGraphType)
            : this(argument)
        {
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedGraphType, nameof(introspectedGraphType));
            Validation.ThrowIfNull(argument, nameof(argument));
            _rawDefaultValue = argument.DefaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedInputValueType"/> class.
        /// </summary>
        /// <param name="inputField">The field of an input object used to populate this value.</param>
        /// <param name="introspectedGraphType">The meta data representing the type of this argument.</param>
        public IntrospectedInputValueType(IInputGraphField inputField, IntrospectedType introspectedGraphType)
            : this(inputField)
        {
            Validation.ThrowIfNull(inputField, nameof(inputField));
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedGraphType, nameof(introspectedGraphType));
            _rawDefaultValue = inputField.DefaultValue;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="IntrospectedInputValueType" /> class from being created.
        /// </summary>
        /// <param name="argument">The argument being introspected.</param>
        private IntrospectedInputValueType(ISchemaItem argument)
            : base(argument)
        {
        }

        /// <inheritdoc />
        public override void Initialize(IntrospectedSchema schema)
        {
            if (_rawDefaultValue == null)
                return;

            if (_rawDefaultValue is bool boolValue)
            {
                // microsoft returns "True" and "False" for boolean conversions to string
                // #sad face
                this.DefaultValue = boolValue.ToString().ToLowerInvariant();
            }
            else if (_rawDefaultValue is string stringValue)
            {
                // graphql requires and defaultValue parameters be encoded as a string
                // see spec: https://graphql.github.io/graphql-spec/October2021/#sec-The-__InputValue-Type
                // any strings must be escaped to be treated as a string
                // e.g. convert "myString" => "\"myString\""
                var delimiter = stringValue.Contains("\n") ? ParserConstants.BLOCK_STRING_DELIMITER : ParserConstants.NORMAL_STRING_DELIMITER;
                this.DefaultValue = $"{delimiter}{stringValue}{delimiter}";
            }
            else if (this.IntrospectedGraphType.Kind == TypeKind.ENUM)
            {
                this.DefaultValue = schema.Schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatEnumValueName(_rawDefaultValue?.ToString());
            }
            else
            {
                this.DefaultValue = _rawDefaultValue.ToString();
            }
        }

        /// <summary>
        /// Gets the data model item representing the <see cref="IGraphType"/> this input value is an instance of.
        /// </summary>
        /// <value>The type.</value>
        public IntrospectedType IntrospectedGraphType { get; }

        /// <summary>
        /// Gets the default value of this argument, represented as a string.
        /// </summary>
        /// <value>The default value.</value>
        public string DefaultValue { get; private set; }
    }
}