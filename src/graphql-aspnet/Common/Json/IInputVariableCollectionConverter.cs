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
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A object that can convert a <see cref="JsonDocument"/> into a <see cref="IInputVariableCollection"/>
    /// for use during a query resolution.
    /// </summary>
    public class IInputVariableCollectionConverter : JsonConverter<IInputVariableCollection>
    {
        private readonly InputVariableCollectionConverter _baseConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="IInputVariableCollectionConverter"/> class.
        /// </summary>
        public IInputVariableCollectionConverter()
        {
            _baseConverter = new InputVariableCollectionConverter();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IInputVariableCollection value, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{nameof(IInputVariableCollection)} cannot be serialized");
        }

        /// <inheritdoc />
        public override IInputVariableCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return _baseConverter.Read(ref reader, typeof(InputVariableCollection), options);
        }
    }
}