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
    using GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveInvocationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class TypeSystemDirectiveInvocationTests
    {
        [Test]
        public void ObjectLevelDirective_IsInvokedExactlyOnce()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<ObjectForDirectiveInvocation>()
                .Build();

            Assert.AreEqual(1, ObjectDirectiveToInvoke.TotalInvocations);
        }
    }
}