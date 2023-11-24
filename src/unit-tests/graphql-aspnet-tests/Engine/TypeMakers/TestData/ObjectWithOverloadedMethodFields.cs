// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    public class ObjectWithOverloadedMethodFields
    {
        [GraphField]
        public int Field1(int arg1)
        {
            return 0;
        }

        public int Field1(string arg2)
        {
            return 0;
        }
    }
}