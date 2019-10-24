// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules
{
    /// <summary>
    /// A publically exposed core object for housing common constants and other rarely changed values relating
    /// to all internal rule processors.
    /// </summary>
    public static class RuleProcessor
    {
        /// <summary>
        /// Gets or sets the maximum processing depth supported by the rule processing engine. This value can be safely changed
        /// during startup but should not be edited at runtime.
        /// </summary>
        /// <value>The maximum processing depth.</value>
        public static int MaxProcessingDepth { get; set; }

        /// <summary>
        /// Initializes static members of the <see cref="RuleProcessor" /> class.
        /// </summary>
        static RuleProcessor()
        {
            MaxProcessingDepth = 150;
        }
    }
}