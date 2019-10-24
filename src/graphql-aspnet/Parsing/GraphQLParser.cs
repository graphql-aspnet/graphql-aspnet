// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing
{
    using System;
    using System.Text;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using KEYWORDS = ParserConstants.Keywords;

    /// <summary>
    /// A parse that will convert a graphql query into valid
    /// syntax tree to be used by the a formal schema for fulfilling the request.
    /// </summary>
    public class GraphQLParser : IGraphQLDocumentParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLParser"/> class.
        /// </summary>
        public GraphQLParser()
        {
        }

        /// <summary>
        /// Takes in a raw query and converts into an executable document according to
        /// its internal rule set.  If, during parsing, an error occurs or something about
        /// the supplied query text is incorrect or unexpected a <see cref="GraphQLSyntaxException" />.
        /// </summary>
        /// <param name="queryText">The raw query text to be parsed.</param>
        /// <returns>The completed document.</returns>
        public ISyntaxTree ParseQueryDocument(ReadOnlyMemory<char> queryText)
        {
            // if an exception occurs during parsing just let it bubble up
            // the owner of the parse request will handle it accordingly
            // ...usually by return an error in the query result
            //
            // ----------------------------------
            // Step 1: Parse and Tokenization
            // -------
            // Attempt to take the text and turn it into a series of structurally valid
            // tokens according to the graph ql specification.
            //
            // Lexing provides the following garuntees
            // - - - - - - - - - - - -
            // 1) Strings are garunteed to be valid, properly delimited and with validly escaped chars
            //    1a) Delimiters are not removed; single-quotes or triple-quotes are still attached
            //    1b) escaped characters are still escaped and not yet converted to their real unicode characters
            // 2) Numbers are garunteed to be in a format dictated by the specification but have not been parsed as numbers
            // 3) All control characters are vetted as valid and syntactically correct
            //    3a) matching braces ARE NOT garunteed yet.
            // 4) Any unecessary whitespace or characters has already been removed
            //    4a) Non-relaivent "valid but useless' control characters (like commas) have not been removed
            // 5) Single line comments have been parsed but the delimiters have not been removed
            //    5a) All comments start with '#' and are always only one line
            // ----------------------------------
            var source = new SourceText(queryText);
            var tokenStream = Lexer.Tokenize(source);
            var syntaxTree = new SyntaxTree();

            // ----------------------------------
            // Step 2: Process the Document
            // -------
            // Analyze the tokens to ensure they form a real query and build the graph document as processing occurs.
            //
            // Examples of operations performed (not an exhaustive list)
            // * Ensure names and strings fall where they should
            // * Parens, brackets and brace are properly opened and closed
            // * received strings or numbers are paired correctly with their named values
            // * graph projections appear as they should (after an option)
            // * mutation, query and subscription definition rules are enforced
            // * variables are identified and proper reference is ensured
            // * ..and on and on
            // ----------------------------------
            while (!tokenStream.EndOfStream)
            {
                // offload processing of the queue to a specialized top-level-node "maker"
                // based on the keyword at the top of the stream .
                // Load the correct maker and let that process the stream to generate
                // the next segment from the source text, leaving the stream in a state such
                // that the next segment could be generated
                //
                // As a rule of thumb, ALL makers will ensure that the stream
                // is pointing at a token that indicates the start
                // of the node or nodes they are responsible to build
                // and that when finished, the stream is beyond the end of the
                // last known token that could be part of the node
                // the maker generates. This is usually a close brace
                // ---------
                // a fragment node MUST start with the keyword 'fragment'
                // see spec: section 2.8
                // ---------
                // a query or mutation MAY start with a keyword declaration
                // or skipping all that and variables, just be an open brace
                // see spec: section 2.3 "query shorthand"
                ISyntaxNodeMaker maker;
                if (tokenStream.Match(KEYWORDS.Fragment))
                {
                    maker = NodeMakerFactory.CreateMaker<NamedFragmentNode>();
                }
                else
                {
                    maker = NodeMakerFactory.CreateMaker<OperationTypeNode>();
                }

                var node = maker.MakeNode(tokenStream);
                syntaxTree.AddNode(node);
            }

            return syntaxTree;
        }

        /// <summary>
        /// Strips extra whitespace from the query text and replaces any escaped whitespace characters
        /// with a space (i.e. \n, \r, \t etc.)
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <returns>System.String.</returns>
        public string StripInsignificantWhiteSpace(string queryText)
        {
            var whiteSpaceSpan = ParserConstants.Characters.WhiteSpace.Span;

            var builder = new StringBuilder();
            builder.Append(queryText[0]);
            for (var i = 1; i < queryText.Length; i++)
            {
                var isWhiteSpace = false;
                if (whiteSpaceSpan.IndexOf(queryText[i]) >= 0)
                {
                    isWhiteSpace = true;
                    if (whiteSpaceSpan.IndexOf(queryText[i - 1]) >= 0)
                    {
                        continue;
                    }
                }

                builder.Append(isWhiteSpace ? ' ' : queryText[i]);
            }

            return builder.ToString();
        }
    }
}