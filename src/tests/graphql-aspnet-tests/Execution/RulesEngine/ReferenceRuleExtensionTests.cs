// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.RulesEngine
{
    using GraphQL.AspNet.RulesEngine;
    using NUnit.Framework;

    [TestFixture]
    public class ReferenceRuleExtensionTests
    {
        [TestCase("myTag", Constants.SPECIFICATION_URL + "#myTag")]
        [TestCase("  myTag  ", Constants.SPECIFICATION_URL + "#myTag")]
        [TestCase("#myTag", Constants.SPECIFICATION_URL + "#myTag")]
        [TestCase("  #myTag  ", Constants.SPECIFICATION_URL + "#myTag")]
        [TestCase("", Constants.SPECIFICATION_URL)]
        [TestCase(null, Constants.SPECIFICATION_URL)]
        public void AnchorTag(string input, string expectedOutput)
        {
            var result = ReferenceRule.CreateFromAnchorTag(input);

            Assert.AreEqual(expectedOutput, result);
        }
    }
}