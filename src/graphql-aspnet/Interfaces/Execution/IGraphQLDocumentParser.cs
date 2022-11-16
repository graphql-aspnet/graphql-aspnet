// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;

    /// <summary>
    /// Represents an object that is capable of converted a raw block of text into a parsed
    /// set of instructions, a <see cref="IGraphQueryDocument"/>, that can be acted on and executed.
    /// </summary>
    public interface IGraphQLDocumentParser
    {
        /// <summary>
        /// Takes in a raw query and converts into an executable document according to
        /// its internal rule set.  If, during parsing, an error occurs or something about
        /// the supplied query text is incorrect or unexpected a <see cref="GraphQLSyntaxException"/>.
        /// </summary>
        /// <param name="queryText">The raw query text to be parsed.</param>
        /// <returns>The completed document.</returns>
        SyntaxTree ParseQueryDocument(ref SourceText queryText);

        /// <summary>
        /// Strips extra whitespace from the query text.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <returns>System.String.</returns>
        string StripInsignificantWhiteSpace(string queryText);
    }
}