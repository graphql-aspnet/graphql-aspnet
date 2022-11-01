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
    using GraphQL.AspNet.Parsing2;

    public class SynNodeTestCase
    {
        public static SynNodeTestCase NoChildren { get; } = new SynNodeTestCase(SynNodeType.Empty);

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
             SynNodeType nodeType,
             params SynNodeTestCase[] children)
            : this(nodeType, string.Empty, null, string.Empty, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
             SynNodeType nodeType = SynNodeType.Empty,
             string primaryText = "",
             params SynNodeTestCase[] children)
            : this(nodeType, primaryText, null, string.Empty, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
          SynNodeType nodeType,
          string primaryText,
          ScalarValueType primaryValueType,
          params SynNodeTestCase[] children)
         : this(nodeType, primaryText, primaryValueType, string.Empty, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
            SynNodeType? nodeType = null,
            string primaryText = "",
            string secondaryText = "",
            params SynNodeTestCase[] children)
         : this(nodeType, primaryText, null, secondaryText, children)
        {
        }

        [DebuggerStepperBoundary]
        public SynNodeTestCase(
            SynNodeType? nodeType = null,
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

        public SynNodeType? NodeType { [DebuggerStepThrough] get; }

        public string PrimaryText { [DebuggerStepThrough] get; }

        public string SecondaryText { [DebuggerStepThrough] get; }

        public SynNodeTestCase[] Children { [DebuggerStepThrough] get; }

        public ScalarValueType? PrimaryValueType { [DebuggerStepThrough] get; }
    }
}