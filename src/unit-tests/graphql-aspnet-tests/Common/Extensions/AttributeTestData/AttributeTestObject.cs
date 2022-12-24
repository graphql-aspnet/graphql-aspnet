// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Extensions.AttributeTestData
{
    public class AttributeTestObject
    {
        [InheritsFromBaseTest]
        [Inherits2FromBaseTest]
        public void MethodWithAttribute()
        {
        }

        [InheritsFromBaseTest]
        [InheritsFromBaseTest]
        public void MethodWithTwoInstancesOfAttribute()
        {
        }

        [OtherAttribute]
        public void MethodWithNotInheritedAttributeAttribute()
        {
        }

        public void MethodWithoutAttribute()
        {
        }
    }
}