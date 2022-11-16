// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;

    public class SynNodeTestCase
    {
        public static SynNodeTestCase NoChildren { get; } = new SynNodeTestCase(SyntaxNodeType.Empty);

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
             SyntaxNodeType nodeType,
             params SynNodeTestCase[] children)
            : this(nodeType, string.Empty, null, string.Empty, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
             SyntaxNodeType nodeType = SyntaxNodeType.Empty,
             string primaryText = "",
             params SynNodeTestCase[] children)
            : this(nodeType, primaryText, null, string.Empty, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
          SyntaxNodeType nodeType,
          string primaryText,
          ScalarValueType primaryValueType,
          params SynNodeTestCase[] children)
         : this(nodeType, primaryText, primaryValueType, string.Empty, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
            SyntaxNodeType? nodeType = null,
            string primaryText = "",
            string secondaryText = "",
            params SynNodeTestCase[] children)
         : this(nodeType, primaryText, null, secondaryText, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
            SyntaxNodeType? nodeType = null,
            string primaryText = "",
            ScalarValueType? primaryValueType = null,
            string secondaryText = "",
            params SynNodeTestCase[] children)
        {
            this.NodeType = nodeType;
            this.PrimaryText = primaryText;
            this.SecondaryText = secondaryText;
            this.Children = children;
            this.PrimaryValueType = primaryValueType;
        }

        public SyntaxNodeType? NodeType
        {
            [DebuggerStepThrough]
            get;
        }

        public string PrimaryText
        {
            [DebuggerStepThrough]
            get;
        }

        public string SecondaryText
        {
            [DebuggerStepThrough]
            get;
        }

        public SynNodeTestCase[] Children
        {
            [DebuggerStepThrough]
            get;
        }

        public ScalarValueType? PrimaryValueType
        {
            [DebuggerStepThrough]
            get;
        }
    }
}