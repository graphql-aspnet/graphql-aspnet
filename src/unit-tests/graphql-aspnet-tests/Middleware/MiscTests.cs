// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Middleware
{
    using GraphQL.AspNet.Middleware.Exceptions;
    using NUnit.Framework;

    [TestFixture]
    public class MiscTests
    {
        [Test]
        public void ComponentNamePassedToException_RendersInProperty()
        {
            var exception = new GraphPipelineMiddlewareInvocationException("componentName", "The message");
            Assert.AreEqual("componentName", exception.ComponentName);
            Assert.AreEqual("The message", exception.Message);
        }
    }
}