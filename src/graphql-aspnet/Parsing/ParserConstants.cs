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

    /// <summary>
    /// A set of constants scoped to the library.
    /// </summary>
    internal static class ParserConstants
    {
        /// <summary>
        /// Gets a set of characters that, together, indicate an escaped block string.
        /// </summary>
        /// <value>The block string delimiter.</value>
        public const string BLOCK_STRING_DELIMITER = "\"\"\"";

        /// <summary>
        /// Gets a set of characters that, together, indicate an escaped normal string.
        /// </summary>
        /// <value>The normal string delimiter.</value>
        public const string NORMAL_STRING_DELIMITER = "\"";

        /// <summary>
        /// Gets a set of characters that, together, indicate an escaped block string.
        /// </summary>
        /// <value>The block string delimiter.</value>
        internal static ReadOnlyMemory<char> BlockStringDelimiterMemory => BLOCK_STRING_DELIMITER.AsMemory();

        /// <summary>
        /// Gets a set of characters that, together, indicate an escaped normal string.
        /// </summary>
        /// <value>The normal string delimiter.</value>
        internal static ReadOnlyMemory<char> NormalStringDelimiterMemory => NORMAL_STRING_DELIMITER.AsMemory();

        /// <summary>
        /// Reserved keywords that are part of graphql.
        /// </summary>
        public static class Keywords
        {
            public static ReadOnlyMemory<char> Null = "null".AsMemory();
            public static ReadOnlyMemory<char> Query = "query".AsMemory();
            public static ReadOnlyMemory<char> Mutation = "mutation".AsMemory();
            public static ReadOnlyMemory<char> Subscription = "subscription".AsMemory();
            public static ReadOnlyMemory<char> On = "on".AsMemory();
            public static ReadOnlyMemory<char> Fragment = "fragment".AsMemory();
            public static ReadOnlyMemory<char> True = "true".AsMemory();
            public static ReadOnlyMemory<char> False = "false".AsMemory();
        }

        /// <summary>
        /// A collection of known characters constants within a lexical scope.
        /// </summary>
        public static class Characters
        {
            public const char BOM = '\xFEFF'; // 65279; byte-order-mark
            public const char NL = '\n'; // 0x000A
            public const char CR = '\r'; // 0x000D
            public const char SPACE = ' '; // 0x0020
            public const char BACKSPACE = '\b'; // 0x0008
            public const char FORMFEED = '\f'; // 0x000C
            public const char TAB = '\t'; // 0x0009
            public const char NUL = '\0'; // 0x0000
            public const char DOUBLE_QUOTE = '"'; // 0x0022
            public const char ESCAPED_CHAR_INDICATOR = '\\'; // 0x005C
            public const int NO_INDEX = -1;

            /// <summary>
            /// Gets the set of characters that can be preceeded by a '\' and will be considered escaped
            /// and in need of translation. All other characters are not valid.
            /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Appendix-Grammar-Summary.Lexical-Tokens .
            /// </summary>
            /// <value>The valid escapable characters.</value>
            public static ReadOnlyMemory<char> ValidEscapableCharacters { get; } = "/bfnrt\\\"".AsMemory();

            /// <summary>
            /// Gets a set of characters that mark an escaped unicode character in a string.
            /// </summary>
            /// <value>The unicode prefix.</value>
            public static ReadOnlyMemory<char> UnicodePrefix { get; } = (ESCAPED_CHAR_INDICATOR + "u").AsMemory();

            /// <summary>
            /// Gets a set of known white space characters.
            /// </summary>
            /// <value>The white space.</value>
            public static ReadOnlyMemory<char> WhiteSpace { get; } = new[] { NL, CR, SPACE, TAB, BOM };

            /// <summary>
            /// Gets a set of characters known to be allowed in a number.
            /// </summary>
            /// <value>The valid digit chars.</value>
            public static ReadOnlyMemory<char> ValidDigitChars { get; } = "1234567890.-eE".AsMemory();

            /// <summary>
            /// Gets a set of characters that indicate the number is a float instead of an integer.
            /// </summary>
            /// <value>The float indicator digits.</value>
            public static ReadOnlyMemory<char> FloatIndicatorChars { get; } = ".eE".AsMemory();

            /// <summary>
            /// Gets a set of characters that, together, make a spread operator.
            /// </summary>
            /// <value>The spread operator.</value>
            public static ReadOnlyMemory<char> SpreadOperator { get; } = new[] { '.', '.', '.' };
        }
    }
}