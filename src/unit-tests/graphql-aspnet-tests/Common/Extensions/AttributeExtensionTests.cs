// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Extensions
{
    using GraphQL.AspNet.Common.Extensions;
    using Microsoft.AspNetCore.Authorization;
    using NUnit.Framework;

    [TestFixture]
    public class AttributeExtensionTests
    {
        [Test]
        public void CanBeAppliedMultipleTimes_MultipleCopyAttribute_ReturnsTrue()
        {
            var attrib = new AuthorizeAttribute("bob");

            var result = attrib.CanBeAppliedMultipleTimes();

            Assert.IsTrue(result);
        }

        [Test]
        public void CanBeAppliedMultipleTimes_SingleCopyAttribute_ReturnsFalse()
        {
            var attrib = new AllowAnonymousAttribute();

            var result = attrib.CanBeAppliedMultipleTimes();

            Assert.IsFalse(result);
        }
    }
}