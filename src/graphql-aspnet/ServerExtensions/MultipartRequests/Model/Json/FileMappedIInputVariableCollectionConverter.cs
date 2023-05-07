// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests.Model.Json
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Execution.Variables.Json;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A special instance of the <see cref="IInputVariableCollectionConverter"/> that can inject
    /// file upload variables into the created collection.
    /// </summary>
    public class FileMappedIInputVariableCollectionConverter : JsonConverter<IInputVariableCollection>
    {
        private readonly FileMappedInputVariableCollectionConverter _baseConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMappedIInputVariableCollectionConverter"/> class.
        /// </summary>
        public FileMappedIInputVariableCollectionConverter()
        {
            _baseConverter = new FileMappedInputVariableCollectionConverter();
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