// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing.Helpers
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;

    internal class LexicalTokenTestCase
    {
        [DebuggerStepperBoundary]
        public LexicalTokenTestCase(TokenType tokenType, string text = null, SourceLocation? location = null, bool isIgnored = false)
        {
            this.TokenType = tokenType;
            this.Text = text;
            this.Location = location;
            this.IsIgnored = isIgnored;
        }

        public TokenType TokenType { get; }

        public string Text { get; }

        public SourceLocation? Location { get; }

        public bool? IsIgnored { get; }
    }
}