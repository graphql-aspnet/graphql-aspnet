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
    using System.ComponentModel;

    /// <summary>
    /// An enumeration representing the possible control characters in a graphql document.
    /// </summary>
    public enum TokenType
    {
        [Description("")] None = 0,

        [Description(TokenTypeNames.STRING_COMMENT)] Comment = 35,

        [Description(TokenTypeNames.STRING_BANG)] Bang = 33,

        [Description(TokenTypeNames.STRING_DOLLAR)] Dollar = 36,

        [Description(TokenTypeNames.STRING_PARENT_LEFT)] ParenLeft = 40,
        [Description(TokenTypeNames.STRING_PAREN_RIGHT)] ParenRight = 41,
        [Description(TokenTypeNames.STRING_COMMA)] Comma = 44,

        [Description(TokenTypeNames.STRING_EQUALS_SIGN)] EqualsSign = 61,
        [Description(".")] SpreadOperatorInitiator = 46,
        [Description("...")] SpreadOperator = 1,

        [Description(TokenTypeNames.STRING_COLON)] Colon = 58,
        [Description(TokenTypeNames.STRING_AT_SYMBOL)] AtSymbol = 64,

        [Description(TokenTypeNames.STRING_BRACKET_LEFT)] BracketLeft = 91,
        [Description(TokenTypeNames.STRING_BRACKET_RIGHT)] BracketRight = 93,

        [Description(TokenTypeNames.STRING_CURLY_BRACE_LEFT)] CurlyBraceLeft = 123,
        [Description(TokenTypeNames.STRING_CURLY_BRACE_RIGHT)] CurlyBraceRight = 125,

        [Description(TokenTypeNames.STRING_PIPE)] Pipe = 124,

        [Description("name")] Name = 2,

        [Description("int")] Integer = 3,
        [Description("float")] Float = 4,
        [Description("string")] String = 5,

        [Description("<EOF>")] EndOfFile = 6,
        [Description("<null>")] Null = 7,
    }
}