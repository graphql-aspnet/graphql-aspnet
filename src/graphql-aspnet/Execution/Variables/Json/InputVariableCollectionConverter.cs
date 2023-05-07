// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables.Json
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A object that can convert a <see cref="JsonDocument"/> into a <see cref="IInputVariableCollection"/>
    /// for use during a query resolution.
    /// </summary>
    public class InputVariableCollectionConverter : JsonConverter<InputVariableCollection>
    {
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, InputVariableCollection value, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{nameof(IInputVariableCollection)} cannot be serialized");
        }

        /// <inheritdoc />
        public override InputVariableCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = this.CreateCollection();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();
                    reader.Read();

                    var variable = this.CreateInputVariable(result, name, ref reader);
                    result.Add(variable);
                }

                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new InvalidOperationException(
                        "Error in Input Collection deserialization. Expected " +
                        $"'{JsonTokenType.EndObject}' but got '{reader.TokenType}'");
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a single input variable out of the given token expanding the token into
        /// collections of appropriate child type variables as needed.
        /// </summary>
        /// <param name="variableCollection">The master variable collection being built.</param>
        /// <param name="name">The name to assign the variable.</param>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <returns>IInputVariable.</returns>
        protected virtual IInputVariable CreateInputVariable(IInputVariableCollection variableCollection, string name, ref Utf8JsonReader reader)
        {
            IInputVariable variable;
            try
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Null:
                        variable = this.CreateNullVariable(name);
                        break;

                    case JsonTokenType.StartArray:
                        variable = this.CreateListVariable(variableCollection, name, ref reader);
                        break;

                    case JsonTokenType.StartObject:
                        variable = this.CreateFieldSetVariable(variableCollection, name, ref reader);
                        break;

                    case JsonTokenType.False:
                    case JsonTokenType.True:
                        var boolValue = reader.GetBoolean();
                        variable = this.CreateBooleanVariable(name, boolValue);
                        break;

                    case JsonTokenType.String:
                        var str = reader.GetString();
                        variable = this.CreateStringVariable(name, str);
                        break;

                    case JsonTokenType.Number:
                        var number = reader.GetDouble();
                        variable = this.CreateNumberVariable(name, number);
                        break;

                    default:
                        throw new FormatException($"Unsupported JsonToken '{reader.TokenType.ToString()}', cannot " +
                            "deserialize variables collection.");
                }
            }
            finally
            {
                reader.Read();
            }

            this.OnVariableCreated(variableCollection, variable);
            return variable;
        }

        /// <summary>
        /// When overridden in a child class allows a method to inspect and interact with a fully formed variable just
        /// prior to it being added to its parent collection, fieldset or array.
        /// </summary>
        /// <param name="variableCollection">The master variable collection being built.</param>
        /// <param name="variable">The variable.</param>
        protected virtual void OnVariableCreated(IInputVariableCollection variableCollection, IInputVariable variable)
        {
        }

        /// <summary>
        /// Creates a new input variable representing the given string value.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The string value to encapsulate as a string value.</param>
        /// <returns>IInputVariable.</returns>
        protected virtual IInputVariable CreateStringVariable(string name, string value)
        {
            // re-delimite the read value to ensure it contains the necessary quotes.
            var delimitedValue = $"\"{value}\"";
            return new InputSingleValueVariable(name, delimitedValue);
        }

        /// <summary>
        /// Creates a new input variable representing the given numeric value.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="number">The number to represent in the variable.</param>
        /// <returns>IInputVariable.</returns>
        protected virtual IInputVariable CreateNumberVariable(string name, double number)
        {
            return new InputSingleValueVariable(name, number.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates a new input variable representing a given boolean value.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="boolValue">The boolean value to encapsulate in the variable.</param>
        /// <returns>IInputVariable.</returns>
        protected virtual IInputVariable CreateBooleanVariable(string name, bool boolValue)
        {
            return new InputSingleValueVariable(name, boolValue.ToString().ToLower());
        }

        /// <summary>
        /// Creates an input variable with the given name that represents a null value.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>IInputVariable.</returns>
        protected virtual IInputVariable CreateNullVariable(string name)
        {
            return new InputNullValueVariable(name);
        }

        /// <summary>
        /// Creates a list variable by creating individual variables for each item of the JArray
        /// and adding them to a collection.
        /// </summary>
        /// <param name="variableCollection">The master variable collection being built.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <returns>IListInputVariable.</returns>
        protected virtual IInputListVariable CreateListVariable(IInputVariableCollection variableCollection, string name, ref Utf8JsonReader reader)
        {
            var result = new InputListVariable(name);

            // consume start of array
            reader.Read();

            var i = 0;
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                var variable = this.CreateInputVariable(variableCollection, i.ToString(), ref reader);
                result.AddVariable(variable);
                i++;
            }

            return result;
        }

        /// <summary>
        /// Creates a new collection to fill during parsing.
        /// </summary>
        /// <returns>InputVariableCollection.</returns>
        protected virtual InputVariableCollection CreateCollection()
        {
            return new InputVariableCollection();
        }

        /// <summary>
        /// Creates a field set variable by inspecting each property of the object and creating individual
        /// variables out of them.
        /// </summary>
        /// <param name="variableCollection">The master variable collection being built.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <returns>IFieldSetInputVariable.</returns>
        protected virtual IInputFieldSetVariable CreateFieldSetVariable(IInputVariableCollection variableCollection, string name, ref Utf8JsonReader reader)
        {
            var result = new InputFieldSetVariable(name);

            // consume start of object
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var propName = reader.GetString();
                reader.Read(); // consume prop name

                var variable = this.CreateInputVariable(variableCollection, propName, ref reader);
                result.AddVariable(variable);
            }

            return result;
        }
    }
}