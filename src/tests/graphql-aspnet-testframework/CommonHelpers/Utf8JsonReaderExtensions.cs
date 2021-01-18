// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.CommonHelpers
{
    using System;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// Deserialization helpers for converting json data.
    /// </summary>
    public static class Utf8JsonReaderExtensions
    {
        /// <summary>
        /// Reads the object pointed at by the reader up to and including its closing
        /// object token and returns the value as a qualified json string. An exception is thrown
        /// if the reader is not pointing to a object start token type. The reader is advanced forward
        /// during this process.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>a qualified json string.</returns>
        public static string ReadObjectAsJsonString(this ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException("Unable to read a json object. The reader is not " +
                    $"currently pointing at {nameof(JsonTokenType.StartObject)} token type.");
            }

            StringBuilder builder = new StringBuilder();

            // append the opening object tag
            builder.Append("{");
            reader.Read();
            var hasWrittenAtLeastOneProperty = false;

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                // comma seperate successive properties
                if (reader.TokenType == JsonTokenType.PropertyName && hasWrittenAtLeastOneProperty)
                {
                    builder.Append(",");
                }

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        builder.Append("\"");
                        builder.Append(reader.GetString());
                        builder.Append("\"");
                        builder.Append(" : ");
                        hasWrittenAtLeastOneProperty = true;
                        reader.Read();
                        break;

                    case JsonTokenType.StartObject:
                        builder.Append(reader.ReadObjectAsJsonString());
                        break;

                    case JsonTokenType.StartArray:
                        builder.Append(reader.ReadArrayAsJsonString());
                        break;

                    default:
                        bool valueRead = reader.ReadValueAsJsonString(out var data);
                        if (valueRead)
                            builder.Append(data);
                        else
                            reader.Read();
                        break;
                }
            }

            // read the final close tag
            builder.Append("}");
            reader.Read();

            return builder.ToString();
        }

        /// <summary>
        /// Attempts to read the current token as a flat value, converting its contents to a string
        /// representation of an appropriate value. Actual string data is
        /// automatically quoted. If the token is not readable or doesn't produce a value, false is returned
        /// and the token IS NOT consumed (such as with comments or 'none' token types). If the value is successfully
        /// read the token IS consumed in the process.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="data">The parameter populated with the read string value
        /// when it is successfully read.</param>
        /// <returns>System.String.</returns>
        public static bool ReadValueAsJsonString(this ref Utf8JsonReader reader, out string data)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    data = "{";
                    reader.Read();
                    return true;

                case JsonTokenType.EndObject:
                    data = "}";
                    reader.Read();
                    return true;

                case JsonTokenType.StartArray:
                    data = "[";
                    reader.Read();
                    return true;

                case JsonTokenType.EndArray:
                    data = "]";
                    reader.Read();
                    return true;

                case JsonTokenType.String:
                    data = $"\"{reader.GetString()}\" ";
                    reader.Read();
                    return true;

                case JsonTokenType.Number:
                    data = Encoding.UTF8.GetString(reader.ValueSpan.ToArray());
                    reader.Read();
                    return true;

                case JsonTokenType.True:
                    data = "true";
                    reader.Read();
                    return true;

                case JsonTokenType.False:
                    data = "false";
                    reader.Read();
                    return true;

                case JsonTokenType.Null:
                    data = "null";
                    reader.Read();
                    return true;

                default:
                    data = null;
                    return false;
            }
        }

        /// <summary>
        /// Reads the object pointed at by the reader up to and including its closing
        /// object token and returns the value as a qualified json string. An exception is thrown
        /// if the reader is not pointing to a object start token type. The reader is advanced forward
        /// during this process.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>a qualified json string.</returns>
        public static string ReadArrayAsJsonString(this ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException("Unable to read a json array. The reader is not " +
                    $"currently pointing at {nameof(JsonTokenType.StartArray)} token type.");
            }

            StringBuilder builder = new StringBuilder();

            // append the opening array tag
            builder.Append("[");
            reader.Read();
            var hasWrittenOneElement = false;

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (hasWrittenOneElement)
                {
                    builder.Append(",");
                }

                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        builder.Append(reader.ReadObjectAsJsonString());
                        break;

                    case JsonTokenType.StartArray:
                        builder.Append(reader.ReadArrayAsJsonString());
                        break;

                    default:
                        bool valueRead = reader.ReadValueAsJsonString(out var data);
                        if (valueRead)
                            builder.Append(data);
                        else
                            reader.Read();
                        break;
                }

                hasWrittenOneElement = true;
            }

            // read the final close tag
            builder.Append("]");
            reader.Read();

            return builder.ToString();
        }
    }
}