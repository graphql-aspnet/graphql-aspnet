// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine
{
    /// <summary>
    /// A publicly editable, core object for housing common constants and other rarely changed values relating
    /// to the internal rule processors.
    /// </summary>
    public static class RuleProcessor
    {
        /// <summary>
        /// Gets or sets the maximum processing depth supported by the rule processing engine.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The rules processing engine is reponsible for executing rule sets of various kinds against parent/child structued
        /// data sets (i.e. <c>entity -> child -> childOfChild</c> etc.) This value indicates the maximum depth of a parent/child tree
        /// that processing will go. If this limit is reached, all processing is immediately stopped and an exception is thrown.
        /// </para>
        /// <para>This value can be safely changed during startup but should not be edited at runtime.</para>
        /// </remarks>
        /// <value>The maximum processing depth.</value>
        public static int MaxProcessingDepth { get; set; }

        /// <summary>
        /// Initializes static members of the <see cref="RuleProcessor" /> class.
        /// </summary>
        static RuleProcessor()
        {
            MaxProcessingDepth = 250;
        }
    }
}