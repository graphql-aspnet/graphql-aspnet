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
    "StyleCop.CSharp.ReadabilityRules",
    "SA1134:Attributes must not share line",
    Justification = "Readability of description text for a given value is improved by single lineing them",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Parsing.Lexing.Tokens.TokenType")]

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

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1025:Code should not contain multiple whitespace in a row",
    Justification = "Enum is cleaner to look at with extra white space",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Schemas.TypeSystem.DirectiveLocation")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1025:Code should not contain multiple whitespace in a row",
    Justification = "Enum is cleaner to look at with extra white space",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Directives.ExecutableDirectiveLocation")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1025:Code should not contain multiple whitespace in a row",
    Justification = "Enum is cleaner to look at with extra white space",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Directives.TypeSystemDirectiveLocation")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.SpacingRules",
    "SA1025:Code should not contain multiple whitespace in a row",
    Justification = "Enum is cleaner to look at with extra white space",
    Scope = "type",
    Target = "~T:GraphQL.AspNet.Directives.DirectiveLifeCyclePhase")]
