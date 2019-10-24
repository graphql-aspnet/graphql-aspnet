// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Response;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Response;

    /// <summary>
    /// A standard implementation that writes the results of an executed document
    /// to a stream.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this writer works for.</typeparam>
    public class DefaultResponseWriter<TSchema> : IGraphResponseWriter<TSchema>
         where TSchema : class, ISchema
    {
        private readonly GraphMessageSeverity _minSeverityLevel;
        private readonly Func<DateTimeOffset, DateTime> _timeLocalizer;
        private readonly GraphNameFormatter _nameFormatter;
        private readonly JsonWriterOptions _writerOptions;
        private readonly JsonSerializerOptions _serializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResponseWriter{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema from which repsonse settings should be drawn.</param>
        public DefaultResponseWriter(TSchema schema)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            _minSeverityLevel = schema.Configuration.ResponseOptions.MessageSeverityLevel;
            _timeLocalizer = schema.Configuration.ResponseOptions.TimeStampLocalizer;
            _nameFormatter = schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            _writerOptions = new JsonWriterOptions()
            {
                Indented = schema.Configuration.ResponseOptions.IndentDocument,
            };

            _serializerSettings = new JsonSerializerOptions();
            _serializerSettings.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }

        /// <summary>
        /// Attempts to write the provided <see cref="IGraphOperationResult" /> to the stream. Generally this stream
        /// will be the response stream for an HTTP request.
        /// </summary>
        /// <param name="streamToWriteTo">The stream to write to.</param>
        /// <param name="resultToWrite">The result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        /// <returns>Task.</returns>
        public async Task WriteAsync(Stream streamToWriteTo, IGraphOperationResult resultToWrite, GraphQLResponseOptions options = null)
        {
            options = options ?? GraphQLResponseOptions.Default;

            Utf8JsonWriter writer = null;
            try
            {
                writer = new Utf8JsonWriter(streamToWriteTo, _writerOptions);
                this.WriteResult(writer, resultToWrite, options);
            }
            finally
            {
                if (writer != null)
                {
                    await writer.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Converts the operation reslt into a dictionary map of the required fields for a graphql response.
        /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Response .
        /// </summary>
        /// <param name="writer">The json writer to output the reslts to.</param>
        /// <param name="resultToWrite">The operation result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        private void WriteResult(Utf8JsonWriter writer, IGraphOperationResult resultToWrite, GraphQLResponseOptions options)
        {
            writer.WriteStartObject();

            if (resultToWrite.Messages.Severity >= _minSeverityLevel)
            {
                writer.WriteStartArray("errors");
                foreach (var message in resultToWrite.Messages.Where(x => x.Severity >= _minSeverityLevel))
                    this.WriteMessage(writer, message, options);

                writer.WriteEndArray();
            }

            if (resultToWrite.Data != null || resultToWrite.Messages.Count == 0)
            {
                writer.WritePropertyName("data");
                this.WriteObjectCollection(writer, resultToWrite.Data);
            }

            if (options.ExposeMetrics && resultToWrite.Metrics != null)
            {
                var result = resultToWrite.Metrics.GenerateResult();
                writer.WritePropertyName("extensions");
                this.WriteObjectCollection(writer, result);
            }

            writer.WriteEndObject(); // {document}
        }

        /// <summary>
        /// Creates a new dictionary of properties for the message map
        /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Errors .
        /// </summary>
        /// <param name="writer">The json writer to output the reslts to.</param>
        /// <param name="message">The message to render into the output.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        private void WriteMessage(Utf8JsonWriter writer, IGraphMessage message, GraphQLResponseOptions options)
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

            var timestamp = _timeLocalizer?.Invoke(message.TimeStamp) ?? message.TimeStamp;
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

        private void WriteResponseItem(Utf8JsonWriter writer, IResponseItem dataItem)
        {
            if (dataItem == null)
            {
                this.WriteLeafValue(writer, dataItem);
                return;
            }

            switch (dataItem)
            {
                case IResponseFieldSet fieldSet:
                    this.WriteObjectCollection(writer, fieldSet);
                    break;
                case IResponseList list:
                    this.WriteList(writer, list);
                    break;
                case IResponseSingleValue singleValue:
                    this.WriteLeafValue(writer, singleValue.Value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        $"Unknown {nameof(IResponseItem)} type. " +
                        $"Default writer is unable to write type '{dataItem.GetType().FriendlyName()}' to the output stream.");
            }
        }

        /// <summary>
        /// Walks the the object collection and writes it to the provided writer.
        /// </summary>
        /// <param name="writer">The json writer to output the reslts to.</param>
        /// <param name="data">The dictionary to output to the writer.</param>
        private void WriteObjectCollection(Utf8JsonWriter writer, IResponseFieldSet data)
        {
            if (data == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();
                foreach (var kvp in data.Fields)
                {
                    writer.WritePropertyName(kvp.Key);
                    this.WriteResponseItem(writer, kvp.Value);
                }

                writer.WriteEndObject();
            }
        }

        private void WriteList(Utf8JsonWriter writer, IResponseList list)
        {
            if (list?.Items == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in list.Items)
                {
                    this.WriteResponseItem(writer, item);
                }

                writer.WriteEndArray();
            }
        }

        /// <summary>
        /// Writes the entire leaf property to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The property name to assign.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="convertUnsupportedToString">When <c>true</c> if the object type
        /// is not natively supported by this method its internally serialized to a string then written to the response.</param>
        private void WriteLeaf(Utf8JsonWriter writer, string name, object value, bool convertUnsupportedToString = false)
        {
            writer.WritePropertyName(name);
            this.WriteLeafValue(writer, value, convertUnsupportedToString);
        }

        /// <summary>
        /// Writes the leaf value provided to the json writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="convertUnsupportedToString">When <c>true</c> if the object type
        /// is not natively supported by this method its internally serialized to a string then written to the response.</param>
        private void WriteLeafValue(Utf8JsonWriter writer, object value, bool convertUnsupportedToString = false)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.GetType().IsEnum)
                value = _nameFormatter.FormatEnumValueName(value.ToString());

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

                default:
                    if (convertUnsupportedToString)
                    {
                        var output = JsonSerializer.Serialize(value, _serializerSettings);
                        writer.WriteStringValue(output);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Unknown type. Default writer is unable to write " +
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
        private void WritePreEncodedStringValue(Utf8JsonWriter writer, string value)
        {
            writer.WriteStringValue(JsonEncodedText.Encode(value, _serializerSettings.Encoder));
        }

        /// <summary>
        /// Performs a serialization of the value to a minimally escaped UTF-8 and writes the string
        /// property to the writer. Prevents over-escaping caused by <see cref="JsonSerializer"/> while still
        /// providing required UTF-8 escaping.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to serialize.</param>
        private void WritePreEncodedString(Utf8JsonWriter writer, string propertyName, string value)
        {
            writer.WriteString(propertyName, JsonEncodedText.Encode(value, _serializerSettings.Encoder));
        }
    }
}