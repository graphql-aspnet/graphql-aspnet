// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class TypeCreationItem
    {
        [GraphField]
        public double Method1(string arg1, int arg2, int arg3 = 5)
        {
            return double.MinValue;
        }

        [GraphField]
        public TwoPropertyObject Method2(long arg1, decimal? arg2)
        {
            return null;
        }

        [GraphField]
        public string Prop1 { get; set; }
    }
}