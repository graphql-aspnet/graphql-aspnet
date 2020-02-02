// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Messaging
{
    /// <summary>
    /// A custom serializer for <see cref="System.Text.Json.JsonSerializer"/> to properly
    /// decode a string to an <see cref="GraphQLOperationMessage"/>. Normalizes various formats
    /// of message type strings and null, empty an optional payloads.
    /// </summary>
    internal class OperationMessageDeserializer
    {
    }
}