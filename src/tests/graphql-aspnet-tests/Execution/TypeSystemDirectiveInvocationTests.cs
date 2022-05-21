// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveInvocationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class TypeSystemDirectiveInvocationTests
    {
        [Test]
        public async Task AppliedDirective_IsSeenInExecutedQuery()
        {
            var server = new TestServerBuilder()
                .AddGraphType<SarcasticObject>()
                .AddDirective<ToSarcasticCaseDirective>()
                .AddGraphController<SarcasticController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      retrieveSarcasm { prop1 } " +
                "}");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveSarcasm"" : {
                            ""prop1"" : ""sOmE SaRcAsTiC SeNtEnCe""
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public void DirectiveIsInvokedExactlyOnceDuringConstruction()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<ObjectForDirectiveInvocation>()
                .Build();

            Assert.AreEqual(1, ObjectDirectiveToInvoke.TotalInvocations);
        }

        [Test]
        public void ParameterizedDirective_RecievedExpectedParameterValue()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<ParameterizedObjectForDirectiveInvocation>()
                .Build();

            Assert.AreEqual(1, ParameterizedObjectDirectiveToInvoke.TotalInvocations);
            Assert.AreEqual("stringABC", ParameterizedObjectDirectiveToInvoke.LastParam1Value);
            Assert.AreEqual(33, ParameterizedObjectDirectiveToInvoke.LastParam2Value);
        }

        [Test]
        public void ParameterizedDirective_WithInvalidParameterSet_ThrowsException()
        {
            try
            {
                var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                    .AddGraphType<IncorrectParameterizedObjectForDirectiveInvocation>()
                    .Build();
            }
            catch (GraphExecutionException)
            {
            }
            catch
            {
                Assert.Fail("Unexpected Exception thrown");
            }
        }
    }
}