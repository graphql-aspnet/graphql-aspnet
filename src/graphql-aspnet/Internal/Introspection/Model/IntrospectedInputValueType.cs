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
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A representation of data about a graph type being exposed as an input to another field.
    /// </summary>
    [DebuggerDisplay("Introspected Input Value: {Name}")]
    public sealed class IntrospectedInputValueType : IntrospectedItem, ISchemaItem
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
        /// Initializes a new instance of the <see cref="IntrospectedInputValueType" /> class.
        /// </summary>
        /// <param name="inputField">The field of an input object used to populate this value.</param>
        /// <param name="introspectedGraphType">The meta data representing the type of this argument.</param>
        /// <param name="rawDefaultValue">The default value that should be supplied for this input value
        /// when its not supplied on a query.</param>
        public IntrospectedInputValueType(IInputGraphField inputField, IntrospectedType introspectedGraphType, object rawDefaultValue)
            : this(inputField)
        {
            Validation.ThrowIfNull(inputField, nameof(inputField));
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedGraphType, nameof(introspectedGraphType));
            _rawDefaultValue = rawDefaultValue;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="IntrospectedInputValueType" /> class from being created.
        /// </summary>
        /// <param name="schemaItem">The schema item being introspected.</param>
        private IntrospectedInputValueType(ISchemaItem schemaItem)
            : base(schemaItem)
        {
        }

        /// <inheritdoc />
        public override void Initialize(IntrospectedSchema introspectedSchema)
        {
            // graphql requires and defaultValue parameters be encoded as a string
            // in query language syntax
            // see spec: https://graphql.github.io/graphql-spec/October2021/#sec-The-__InputValue-Type
            this.DefaultValue = null;

            if (_rawDefaultValue != null && _rawDefaultValue.GetType() == typeof(NoDefaultValue))
            {
                // case for "no default value supplied"
                this.DefaultValue = null;
            }
            else
            {
                var generator = new QueryLanguageGenerator(introspectedSchema.Schema);
                this.DefaultValue = generator.SerializeObject(_rawDefaultValue);
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