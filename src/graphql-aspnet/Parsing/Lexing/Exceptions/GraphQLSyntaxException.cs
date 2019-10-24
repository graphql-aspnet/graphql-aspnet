// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Exceptions
{
    using System;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// Thrown when an invalid or unexpected character is found while parsing the provided source text.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class GraphQLSyntaxException : Exception
    {
        /// <summary>
        /// A helper to throw a new exception from the given source location, indicating
        /// an invalid token in the document.
        /// </summary>
        /// <param name="location">The location in the source text.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value received.</param>
        public static void ThrowFromExpectation(
            SourceLocation location,
            ReadOnlyMemory<char> expected,
            ReadOnlyMemory<char> actual)
        {
            ThrowFromExpectation(location, expected.ToString(), actual.ToString());
        }

        /// <summary>
        /// Helper method to throw an exception with a message of an expected value vs. a found or supplied value.
        /// </summary>
        /// <param name="location">The location in the source text.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value received.</param>
        public static void ThrowFromExpectation(
            SourceLocation location,
            string expected,
            string actual)
        {
            throw new GraphQLSyntaxException(
                location,
                $"Invalid query. Expected '{expected}' but received '{actual}'");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLSyntaxException" /> class.
        /// </summary>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="message">The message to include.</param>
        /// <param name="innerException">The inner exception, if any.</param>
        public GraphQLSyntaxException(SourceLocation sourceLocation, string message, Exception innerException = null)
        : base(message, innerException)
        {
            this.Location = sourceLocation;
        }

        /// <summary>
        /// Gets the location in the source text where the parse exception occured.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }
    }
}