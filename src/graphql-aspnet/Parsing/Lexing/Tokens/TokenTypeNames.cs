// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Parsing.Lexing.Tokens
{
    /// <summary>
    /// Character constants for common token characters.
    /// </summary>
    internal static class TokenTypeNames
    {
        public const char COMMENT = '#';
        public const char BANG = '!';
        public const char DOLLAR = '$';
        public const char PARENT_LEFT = '(';
        public const char PAREN_RIGHT = ')';
        public const char COMMA = ',';
        public const char EQUALS_SIGN = '=';
        public const char COLON = ':';
        public const char AT_SYMBOL = '@';
        public const char BRACKET_LEFT = '[';
        public const char BRACKET_RIGHT = ']';
        public const char CURLY_BRACE_LEFT = '{';
        public const char CURLY_BRACE_RIGHT = '}';
        public const char PIPE = '|';

        public const string STRING_COMMENT = "#";
        public const string STRING_BANG = "!";
        public const string STRING_DOLLAR = "$";
        public const string STRING_PARENT_LEFT = "(";
        public const string STRING_PAREN_RIGHT = ")";
        public const string STRING_COMMA = ",";
        public const string STRING_EQUALS_SIGN = "=";
        public const string STRING_COLON = ":";
        public const string STRING_AT_SYMBOL = "@";
        public const string STRING_BRACKET_LEFT = "[";
        public const string STRING_BRACKET_RIGHT = "]";
        public const string STRING_CURLY_BRACE_LEFT = "{";
        public const string STRING_CURLY_BRACE_RIGHT = "}";
        public const string STRING_PIPE = "|";
    }
}