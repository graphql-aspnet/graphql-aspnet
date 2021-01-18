// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class IntrospectableObject
    {
        [GraphField]
        public TwoPropertyObject Method1(string arg1 = "bob", int arg2 = 5)
        {
            return null;
        }

        [GraphField]
        [Deprecated("Because Reason")]
        public TwoPropertyObjectV2 Method2()
        {
            return null;
        }

        [GraphField("Prop1")]
        public long Prop1 { get; set; }
    }
}