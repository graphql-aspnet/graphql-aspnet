// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests
{
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using NUnit.Framework;

    [TestFixture]
    public class MultiPartRequestGraphQLPayloadTests
    {
        [Test]
        public void SinglePayload_IsNotBatch()
        {
            var data = new GraphQueryData();

            var payload = new MultiPartRequestGraphQLPayload(data);

            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.AreEqual(data, payload.QueriesToExecute[0]);
        }

        [Test]
        public void MultiplePayloads_IsBatch()
        {
            var data1 = new GraphQueryData();
            var data2 = new GraphQueryData();

            var list = data1.AsEnumerable().Concat(data2.AsEnumerable()).ToList();

            var payload = new MultiPartRequestGraphQLPayload(list);

            Assert.IsTrue(payload.IsBatch);
            Assert.AreEqual(2, payload.QueriesToExecute.Count);
            Assert.AreEqual(data1, payload.QueriesToExecute[0]);
            Assert.AreEqual(data2, payload.QueriesToExecute[1]);
        }
    }
}