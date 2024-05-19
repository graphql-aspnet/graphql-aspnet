// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting.FormatRules
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base class for all the individual name format rules.
    /// </summary>
    public abstract class NameFormatRuleBase
    {
        /// <summary>
        /// Formats or reformats the name according to the rules of this formatter.
        /// </summary>
        /// <param name="name">The name value being formatted.</param>
        /// <param name="strategy">The selected strategy to format with.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatText(string name, TextFormatOptions strategy)
        {
            if (name == null)
                return null;

            switch (strategy)
            {
                case TextFormatOptions.ProperCase:
                    return name.FirstCharacterToUpperInvariant();

                case TextFormatOptions.CamelCase:
                    return name.FirstCharacterToLowerInvariant();

                case TextFormatOptions.UpperCase:
                    return name.ToUpperInvariant();

                case TextFormatOptions.LowerCase:
                    return name.ToLowerInvariant();

                // ReSharper disable once RedundantCaseLabel
                case TextFormatOptions.NoChanges:
                default:
                    return name;
            }
        }

        /// <summary>
        /// Treats the text as if it were to be a graph type name and enforces global type rule
        /// rename restrictions before renaming the text value. The string is returned altered
        /// or unaltered depending on its original value.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <param name="formatOption">The format to apply, if allowed.</param>
        /// <returns>System.String.</returns>
        protected virtual string FormatGraphTypeName(string name, TextFormatOptions formatOption)
        {
            if (GlobalTypes.CanBeRenamed(name))
                name = this.FormatText(name, formatOption);

            return name;
        }
    }
}