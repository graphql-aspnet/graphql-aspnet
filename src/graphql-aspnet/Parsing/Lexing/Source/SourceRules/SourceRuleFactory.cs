// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.Lexing.Source.SourceRules
{
    using System;
    using GraphQL.AspNet.Parsing.Lexing.Source.SourceRules.Rules;

    /// <summary>
    /// Factory container for finding a source rule from an enumeration.
    /// </summary>
    internal static class SourceRuleFactory
    {
        /// <summary>
        /// Finds the rule instance matching the given enumeration. If no match is found, an "unfailable" rule is returned
        /// always resulting in a pass.
        /// </summary>
        /// <param name="sourceRule">The source rule.</param>
        /// <returns>ISourceRule.</returns>
        public static ISourceRule FindRule(GraphQLSourceRule sourceRule)
        {
            switch (sourceRule)
            {
                case GraphQLSourceRule.IsStartOfNameGlyph:
                    return IsStartOfNameGlyph.Instance;

                case GraphQLSourceRule.IsControlGlyph:
                    return IsControlGlyph.Instance;

                case GraphQLSourceRule.IsCommentGlyph:
                    return IsCommentGlyph.Instance;

                case GraphQLSourceRule.IsStringDelimiterGlyph:
                    return IsStringDelimiterGlyph.Instance;

                case GraphQLSourceRule.IsStartOfNumberGlyph:
                    return IsStartOfNumberGlyph.Instance;

                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceRule));
            }
        }
    }
}