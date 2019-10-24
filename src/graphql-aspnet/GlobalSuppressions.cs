// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Error codes are self explaintory",
    Scope = "Type",
    Target = "~T:GraphQL.AspNet.Constants.ErrorCodes")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Error codes are self explaintory",
    Scope = "Type",
    Target = "~T:GraphQL.AspNet.Constants.ScalarNames")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Reserved Names are self explaintory",
    Scope = "Type",
    Target = "~T:GraphQL.AspNet.Constants.ReservedNames")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Error codes are self explaintory",
    Scope = "Type",
    Target = "~T:GraphQL.AspNet.Validation.DocumentValidationConstants.ErrorCodes")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.ReadabilityRules",
    "SA1134:Attributes must not share line",
    Justification = "Readability of description text for a given value is improved by single lineing them",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Parsing.Lexing.Tokens.TokenType")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1206:Declaration keywords must follow order",
    Justification = "Ref must be declared first on ref struct",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Lexing.Source.SourceText")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1206:Declaration keywords must follow order",
    Justification = "Ref must be declared first on ref struct",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Lexing.Lexer")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1008:Opening parenthesis must be spaced correctly",
    Justification = "Named Tuple should be treated as return type not a block",
    Scope = "member",
    Target = "~M:GraphQL.AspNet.Lexing.Source.SourceText.RetreiveIndexInCurrentLine")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1008:Opening parenthesis must be spaced correctly",
    Justification = "Named Tuple should be treated as return type not a block",
    Scope = "member",
    Target = "~M:GraphQL.AspNet.Lexing.Source.SourceText.RetrieveLocationFromPosition(System.Int32)")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1008:Opening parenthesis must be spaced correctly",
    Justification = "Named Tuple should be treated as return type not a block",
    Scope = "member",
    Target = "~M:GGraphQL.AspNet.Lexing.Source.SourceText.RetrieveLineInformation(System.Int32)")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1008:Opening parenthesis must be spaced correctly",
    Justification = "Named Tuple should be treated as return type not a block",
    Scope = "member",
    Target = "~M:GraphQL.AspNet.Lexing.Source.SourceText.RetrieveLineInformation(System.Int32)")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Token Type names are self explanitory.",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Parsing.Lexing.Tokens.TokenTypeNames")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Parser constant characters are self explanitory",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Parsing.ParserConstants")]