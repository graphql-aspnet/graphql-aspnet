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
        /// <param name="argument">The field argument used to populate this input value.</param>
        /// <param name="introspectedGraphType">The meta data representing the type of this argument.</param>
        public IntrospectedInputValueType(IGraphFieldArgument argument, IntrospectedType introspectedGraphType)
            : this()
        {
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedGraphType, nameof(introspectedGraphType));
            Validation.ThrowIfNull(argument, nameof(argument));
            this.Name = argument.Name;
            this.Description = argument.Description;
            _rawDefaultValue = argument.DefaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedInputValueType"/> class.
        /// </summary>
        /// <param name="inputField">The field of an input object used to populate this value.</param>
        /// <param name="introspectedGraphType">The meta data representing the type of this argument.</param>
        public IntrospectedInputValueType(IGraphField inputField, IntrospectedType introspectedGraphType)
            : this()
        {
            Validation.ThrowIfNull(inputField, nameof(inputField));
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedGraphType, nameof(introspectedGraphType));

            this.Name = inputField.Name;
            this.Description = inputField.Description;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="IntrospectedInputValueType"/> class from being created.
        /// </summary>
        private IntrospectedInputValueType()
        {
            this.AppliedDirectives = new AppliedDirectiveCollection(this);
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
                // see spec: https://graphql.github.io/graphql-spec/June2018/#sec-The-__InputValue-Type
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

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <summary>
        /// Gets the default value of this argument, represented as a string.
        /// </summary>
        /// <value>The default value.</value>
        public string DefaultValue { get; private set; }

        /// <inheritdoc />
        [GraphSkip]
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}