// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Common.ExpressionExtensionTestData
{
    public class ExpressionTestObject
    {
        public int SomeMethod(int a, string b)
        {
            return 0;
        }

        public int SomeProperty { get; set; }
    }
}