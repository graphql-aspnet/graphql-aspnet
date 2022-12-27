// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model
{
    using System.Collections;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A model object representing an individual '__InputValue',
    /// created from an field argument or input field.
    /// </summary>
    [DebuggerDisplay("Introspected Input Value: {Name}")]
    public sealed class IntrospectedInputValueType : IntrospectedItem, ISchemaItem
    {
        private readonly object _rawDefaultValue;
        private readonly GraphTypeExpression _inputValueTypeExpression;
        private readonly SchemaItemPath _inputValuePath;

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
            _rawDefaultValue = argument.HasDefaultValue ? argument.DefaultValue : IntrospectionNoDefaultValue.Instance;
            _inputValueTypeExpression = argument.TypeExpression;
            _inputValuePath = argument.Route;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedInputValueType" /> class.
        /// </summary>
        /// <param name="inputField">The field of an input object used to populate this value.</param>
        /// <param name="introspectedGraphType">The meta data representing the type of this argument.</param>
        public IntrospectedInputValueType(IInputGraphField inputField, IntrospectedType introspectedGraphType)
            : this(inputField)
        {
            Validation.ThrowIfNull(inputField, nameof(inputField));
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedGraphType, nameof(introspectedGraphType));
            _rawDefaultValue = IntrospectionNoDefaultValue.Instance;
            _inputValueTypeExpression = inputField.TypeExpression;
            _inputValuePath = inputField.Route;
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
            _inputValueTypeExpression = inputField.TypeExpression;
            _inputValuePath = inputField.Route;
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

            // case for "no default value supplied"
            if (_rawDefaultValue != null && _rawDefaultValue.GetType() == typeof(IntrospectionNoDefaultValue))
                return;

            this.ValidateDefaultValueOrThrow(introspectedSchema.Schema, _rawDefaultValue, _inputValueTypeExpression);

            var generator = new SchemaLanguageGenerator(introspectedSchema.Schema);
            this.DefaultValue = generator.SerializeObject(_rawDefaultValue);
        }

        private void ValidateDefaultValueOrThrow(ISchema schema, object valueToCheck, GraphTypeExpression typeExpression)
        {
            if (typeExpression.IsNonNullable)
            {
                if (valueToCheck == null)
                {
                    if (typeExpression.IsListOfItems)
                    {
                        throw new GraphTypeDeclarationException(
                             $"An item in the list of supplied values for schema item '{_inputValuePath}' was <null> when " +
                             $"the type expression indicated non-null (Type Expression: {typeExpression}).");
                    }
                    else
                    {
                        throw new GraphTypeDeclarationException(
                             $"The supplied default value for schema item '{_inputValuePath}' was <null> when " +
                             $"the type expression indicated non-null (Type Expression: {typeExpression}).");
                    }
                }

                typeExpression = typeExpression.UnWrapExpression();
            }

            if (valueToCheck == null)
                return;

            if (typeExpression.IsListOfItems)
            {
                if (!GraphValidation.IsValidListType(valueToCheck.GetType()))
                {
                    throw new GraphTypeDeclarationException(
                         $"Invalid Default Value. A list of items was expected for the default value of schema item '{_inputValuePath}' " +
                         $"(Type Expression: {typeExpression}). " +
                         $"The provided value of type '{valueToCheck.GetType().FriendlyName()}' is not a valid list.");
                }

                foreach (var item in (IEnumerable)valueToCheck)
                    this.ValidateDefaultValueOrThrow(schema, item, typeExpression.UnWrapExpression());

                return;
            }

            var graphType = schema.KnownTypes.FindGraphType(typeExpression.TypeName);
            if (graphType == null)
            {
                // via templating this exception would be impossible
                // added here as a safeguard in case that is somehow bypassed
                throw new GraphTypeDeclarationException(
                    $"Invalid Default Value. Unable to locate a graph type named '{typeExpression.TypeName}' in the schema " +
                    $"for field '{_inputValuePath}' (Type Expression: {_inputValueTypeExpression}).");
            }

            if (graphType is IEnumGraphType enumGraphType)
            {
                var label = enumGraphType.Values.FindByEnumValue(_rawDefaultValue);
                if (label == null)
                {
                    throw new GraphTypeDeclarationException(
                        "Invalid default ENUM value. The default value set as part of " +
                        $"schema item '{_inputValuePath}' is '{valueToCheck}' which is not a valid " +
                        $"value defined on the ENUM type '{enumGraphType.Name}'. Enum labels not included in the " +
                        $"schema cannot be used as default values for input object fields.");
                }
            }

            if (!graphType.ValidateObject(valueToCheck))
            {
                throw new GraphTypeDeclarationException(
                      $"Invalid default value. The default value for schema item {_inputValuePath} of type '{valueToCheck.GetType().FriendlyName()}' " +
                      $"could not be validated by the graph type '{graphType.Name}'. The supplied default value for the schema item " +
                      $"must be coercible by its associated graph type.");
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