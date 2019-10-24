// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Metrics
{
    /// <summary>
    /// A formalized phase of processing a query.
    /// </summary>
    public static class ApolloExecutionPhase
    {
        /// <summary>
        /// The parsing phase includes reading and tokenizing the user's submitted query string.
        /// </summary>
        public const string PARSING = "Parsing";

        /// <summary>
        /// The validation phase includes analyzing the generated syntax tree from parsing and matching
        /// it to the target schema ensuring that the requested fields, input values, variables etc. all
        /// match and are capable of being executed.
        /// </summary>
        public const string VALIDATION = "Validation";

        /// <summary>
        /// The execution phase includes calling the various resolvers indicated by the query and generating and formatting
        /// real data to return to the caller. Execution DOES NOT include writing a response.
        /// </summary>
        public const string EXECUTION = "Execution";
    }
}