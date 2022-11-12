﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using System.Text.Json;

    /// <summary>
    /// A collection of variables supplied by user to be used when resolving
    /// a query operation.
    /// </summary>
    public partial class InputVariableCollection
    {
        private static readonly JsonSerializerOptions _serializationOptions;

        /// <summary>
        /// Gets a singleton instance of an empty variable collection.
        /// </summary>
        /// <value>The empty.</value>
        public static InputVariableCollection Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="InputVariableCollection"/> class.
        /// </summary>
        static InputVariableCollection()
        {
            Empty = new InputVariableCollection();

            _serializationOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
        }

        /// <summary>
        /// Creates a qualified <see cref="InputVariableCollection"/> from a given json string.
        /// </summary>
        /// <param name="jsonDocument">The json document.</param>
        /// <returns>IInputVariableCollection.</returns>
        public static InputVariableCollection FromJsonDocument(string jsonDocument)
        {
            if (string.IsNullOrWhiteSpace(jsonDocument))
                return InputVariableCollection.Empty;

            return JsonSerializer.Deserialize<InputVariableCollection>(jsonDocument, _serializationOptions);
        }
    }
}