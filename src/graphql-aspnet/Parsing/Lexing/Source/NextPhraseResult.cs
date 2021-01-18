// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Source
{
    /// <summary>
    /// A result used in processing a batch of characters from a source text file.
    /// </summary>
    public enum NextPhraseResult
    {
        /// <summary>
        /// Instructs the source text to continue extracting characters
        /// </summary>
        Continue,

        /// <summary>
        /// Instructs the source text that the phrase is complete
        /// </summary>
        Complete,

        /// <summary>
        /// Instructs the source text that the phrase completed on the previous iteration.
        /// </summary>
        CompleteOnPrevious,
    }
}