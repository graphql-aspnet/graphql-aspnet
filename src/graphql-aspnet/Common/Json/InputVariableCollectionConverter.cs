// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Json
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// A object that can convert a <see cref="JsonDocument"/> into a <see cref="IInputVariableCollection"/>
    /// for use during a query resolution.
    /// </summary>
    public class InputVariableCollectionConverter : JsonConverter<InputVariableCollection>
    {
        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, InputVariableCollection value, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{nameof(InputVariableCollection)} cannot be serialized");
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="InputVariableCollection" />.
        /// </summary>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override InputVariableCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new InputVariableCollection();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();

                while (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();
                    reader.Read();

                    result.Add(this.CreateInputVariable(name, ref reader));
                }

                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new InvalidOperationException(
                        "Error in Input collection deserialization. Expected " +
                        $"'{JsonTokenType.EndObject.ToString()}' but got '{reader.TokenType.ToString()}'");
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a single input variable out of the given token expanding the token into
        /// collections of appropriate child type variables as needed.
        /// </summary>
        /// <param name="name">The name to assign the variable.</param>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <returns>IInputVariable.</returns>
        private IInputVariable CreateInputVariable(string name, ref Utf8JsonReader reader)
        {
            try
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Null:
                        return new InputSingleValueVariable(name, null);

                    case JsonTokenType.StartArray:
                        return this.CreateListVariable(name, ref reader);

                    case JsonTokenType.StartObject:
                        return this.CreateFieldSetVariable(name, ref reader);

                    case JsonTokenType.False:
                    case JsonTokenType.True:
                        var boolValue = reader.GetBoolean();
                        return new InputSingleValueVariable(name, boolValue.ToString().ToLower());

                    case JsonTokenType.String:
                        var delimitedValue = $"\"{reader.GetString()}\"";
                        return new InputSingleValueVariable(name, delimitedValue);

                    case JsonTokenType.Number:
                        var number = reader.GetDouble();
                        return new InputSingleValueVariable(name, number.ToString(CultureInfo.InvariantCulture));

                    default:
                        throw new FormatException($"Unsupported JsonToken '{reader.TokenType.ToString()}', cannot " +
                            $"deserialize variables collection.");
                }
            }
            finally
            {
                reader.Read();
            }
        }

        /// <summary>
        /// Creates a list variable by creating individual variables for each item of the JArray
        /// and adding them to a collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <returns>IListInputVariable.</returns>
        private IInputListVariable CreateListVariable(string name, ref Utf8JsonReader reader)
        {
            var result = new InputListVariable(name);

            // consume start of array
            reader.Read();

            var i = 0;
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                var variable = this.CreateInputVariable(i.ToString(), ref reader);
                result.AddVariable(variable);
                i++;
            }

            return result;
        }

        /// <summary>
        /// Creates a field set variable by inspecting each property of the object and creating individual
        /// variables out of them.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="reader">The reader containing a stream of json tokens.</param>
        /// <returns>IFieldSetInputVariable.</returns>
        private IInputFieldSetVariable CreateFieldSetVariable(string name, ref Utf8JsonReader reader)
        {
            var result = new InputFieldSetVariable(name);

            // consume start of object
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                var propName = reader.GetString();
                reader.Read(); // consume prop name

                var variable = this.CreateInputVariable(propName, ref reader);
                result.AddVariable(variable);
            }

            return result;
        }
    }
}