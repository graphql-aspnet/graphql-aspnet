// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Response
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A class containing many shared methods for writing all or part of a <see cref="IGraphOperationResult"/>
    /// to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    public abstract class BaseResponseWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponseWriter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        protected BaseResponseWriter(ISchema schema)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            this.TimeLocalizer = schema.Configuration.ResponseOptions.TimeStampLocalizer;
            this.NameFormatter = schema.Configuration.DeclarationOptions.GraphNamingFormatter;

            var serializerSettings = new JsonSerializerOptions();
            serializerSettings.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            this.SerializerSettings = serializerSettings;
        }

        /// <summary>
        /// Creates a new dictionary of properties for the message map
        /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Errors .
        /// </summary>
        /// <param name="writer">The json writer to output the reslts to.</param>
        /// <param name="message">The message to render into the output.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        protected virtual void WriteMessage(Utf8JsonWriter writer, IGraphMessage message, GraphQLResponseOptions options)
        {
            writer.WriteStartObject();
            this.WritePreEncodedString(writer, "message", message.Message);

            if (message.Origin.Location != SourceLocation.None)
            {
                writer.WriteStartArray("locations");
                writer.WriteStartObject();
                writer.WriteNumber("line", message.Origin.Location.LineNumber);
                writer.WriteNumber("column", message.Origin.Location.LinePosition);
                writer.WriteEndObject();
                writer.WriteEndArray(); // locations
            }

            if (message.Origin.Path != SourcePath.None)
            {
                writer.WriteStartArray("path");
                foreach (var loc in message.Origin.Path)
                {
                    if (loc is int i)
                        writer.WriteNumberValue(i);
                    else if (loc is string str)
                        this.WritePreEncodedStringValue(writer, str);
                    else
                        writer.WriteNullValue();
                }

                writer.WriteEndArray(); // path
            }

            writer.WriteStartObject("extensions");
            this.WriteLeaf(writer, "code", message.Code);

            var timestamp = this.TimeLocalizer?.Invoke(message.TimeStamp) ?? message.TimeStamp;
            this.WriteLeaf(writer,  "timestamp", timestamp);
            this.WriteLeaf(writer,  "severity", message.Severity);

            if (message.MetaData != null && message.MetaData.Count > 0)
            {
                writer.WriteStartObject("metaData");
                foreach (var item in message.MetaData)
                {
                    this.WriteLeaf(writer, item.Key, item.Value, true);
                }

                writer.WriteEndObject(); // metadata
            }

            if (options.ExposeExceptions && message.Exception != null)
            {
                writer.WriteStartObject("exception");
                this.WriteLeaf(writer, "type", message.Exception.GetType().FriendlyName());
                this.WriteLeaf(writer, "message", message.Exception.Message);

                const int maxStackTrace = 4000;
                var stacktraceMessage = message.Exception.StackTrace;
                if (stacktraceMessage?.Length > maxStackTrace)
                {
                    stacktraceMessage = stacktraceMessage.Substring(0, maxStackTrace);
                    stacktraceMessage += $"[cut at {maxStackTrace} characters]";
                }

                if (!string.IsNullOrWhiteSpace(stacktraceMessage))
                {
                    this.WriteLeaf(writer, "stacktrace", stacktraceMessage);
                }

                writer.WriteEndObject(); // exception
            }

            writer.WriteEndObject(); // extensions
            writer.WriteEndObject(); // message
        }

        /// <summary>
        /// Writes the entire leaf property to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The property name to assign.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="convertUnsupportedToString">When <c>true</c> if the object type
        /// is not natively supported by this method its internally serialized to a string then written to the response.</param>
        protected virtual void WriteLeaf(Utf8JsonWriter writer, string name, object value, bool convertUnsupportedToString = false)
        {
            writer.WritePropertyName(name);
            this.WriteLeafValue(writer, value, convertUnsupportedToString);
        }

        /// <summary>
        /// Writes the leaf value provided to the json writer. This method assumes a coorisponding property name
        /// has already been written.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="convertUnsupportedToString">When <c>true</c> if the object type
        /// is not natively supported by this method its internally serialized to a string then written to the response.</param>
        protected virtual void WriteLeafValue(Utf8JsonWriter writer, object value, bool convertUnsupportedToString = false)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.GetType().IsEnum)
                value = this.NameFormatter.FormatEnumValueName(value.ToString());

            switch (value)
            {
                case string str:
                    this.WritePreEncodedStringValue(writer, str);
                    break;

                case double dob:
                    writer.WriteNumberValue(dob);
                    break;

                case float f:
                    writer.WriteNumberValue(f);
                    break;

                case decimal d:
                    writer.WriteNumberValue(d);
                    break;

                case long l:
                    writer.WriteNumberValue(l);
                    break;

                case ulong ul:
                    writer.WriteNumberValue(ul);
                    break;

                case int i:
                    writer.WriteNumberValue(i);
                    break;

                case uint ui:
                    writer.WriteNumberValue(ui);
                    break;

                case byte b:
                    writer.WriteNumberValue(b);
                    break;

                case sbyte sb:
                    writer.WriteNumberValue(sb);
                    break;

                case bool boo:
                    writer.WriteBooleanValue(boo);
                    break;

                case DateTime dt:
                    this.WritePreEncodedStringValue(writer, dt.ToRfc3339String());
                    break;

                case DateTimeOffset dto:
                    this.WritePreEncodedStringValue(writer, dto.ToRfc3339String());
                    break;

#if NET6_0_OR_GREATER
                case DateOnly dateOnly:
                    this.WritePreEncodedStringValue(writer, dateOnly.ToRfc3339String());
                    break;

                case TimeOnly timeOnly:
                    this.WritePreEncodedStringValue(writer, timeOnly.ToRfc3339String());
                    break;
#endif

                default:
                    if (convertUnsupportedToString)
                    {
                        var output = JsonSerializer.Serialize(value, this.SerializerSettings);
                        writer.WriteStringValue(output);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            "Unknown type. Default writer is unable to write " +
                            $"type '{value.GetType().FriendlyName()}' to the output stream.");
                    }

                    break;
            }
        }

        /// <summary>
        /// Performs a serialization of the value to a minimally escaped UTF-8 and writes the string
        /// property to the writer. Prevents over-escaping caused by <see cref="JsonSerializer"/> while still
        /// providing required UTF-8 escaping.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to serialize.</param>
        protected virtual void WritePreEncodedStringValue(Utf8JsonWriter writer, string value)
        {
            writer.WriteStringValue(JsonEncodedText.Encode(value, this.SerializerSettings.Encoder));
        }

        /// <summary>
        /// Performs a serialization of the value to a minimally escaped UTF-8 and writes the string
        /// property to the writer. Prevents over-escaping caused by <see cref="JsonSerializer"/> while still
        /// providing required UTF-8 escaping.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to serialize.</param>
        protected virtual void WritePreEncodedString(Utf8JsonWriter writer, string propertyName, string value)
        {
            writer.WriteString(propertyName, JsonEncodedText.Encode(value, this.SerializerSettings.Encoder));
        }

        /// <summary>
        /// Gets the time localizer to use when writing time stamp values to the stream.
        /// </summary>
        /// <value>The time localizer.</value>
        protected virtual Func<DateTimeOffset, DateTime> TimeLocalizer { get; }

        /// <summary>
        /// Gets the formatter used when writing graph names to the stream.
        /// </summary>
        /// <value>The name formatter.</value>
        protected virtual GraphNameFormatter NameFormatter { get; }

        /// <summary>
        /// Gets a set of settings to use whenever the serializer needs to be directly invoked.
        /// </summary>
        /// <value>The serializer settings.</value>
        protected virtual JsonSerializerOptions SerializerSettings { get; }
    }
}