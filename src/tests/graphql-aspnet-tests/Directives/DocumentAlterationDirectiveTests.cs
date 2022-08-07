// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Directives.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentAlterationDirectiveTests
    {
        [Test]
        public async Task WhenDirectiveAttemptsToInjectAnInvalidArgument_DocumentFailsValidation()
        {
            var server = new TestServerBuilder()
             .AddType<SimpleExecutionController>()
                 .AddType<ArgumentIntjectorDirective>()
                 .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1 @argInjector, property2 } } }")
                .AddOperationName("Operation1");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
            Assert.AreEqual("5.4.1", result.Messages[0].MetaData["Rule"]);
        }

        [Test]
        public async Task WhenDirectiveAltersTheAsignedFragmentOnASpread_DocumentFailsValidation()
        {
            var server = new TestServerBuilder()
             .AddType<SimpleExecutionController>()
             .AddType<TwoPropertyObjectV2>()
             .AddType<OperationSwapDirective>()
             .Build();

            // swap the "mutation" top level operation to be
            // query via a directive, should fail validation
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"mutation @operationSwap {
                        changeData(id: 5) {
                            property1
                        }
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
            Assert.IsTrue(result.Messages[0].Message.ToLowerInvariant().Contains("invalid referenced operation"));
        }
    }
}
