// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System;
    using GraphQL.AspNet.PlanGeneration.Document;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentPartTypeTests
    {
        [Test]
        public void EnsureAllPartTypesMatchAnInterface()
        {
            foreach (DocumentPartType partType in Enum.GetValues(typeof(DocumentPartType)))
            {
                // only unknown should throw an exception
                if (partType == DocumentPartType.Unknown)
                {
                    Assert.Throws<InvalidOperationException>(() => partType.RelatedInterface());
                    return;
                }

                var foundInterface = partType.RelatedInterface();
                Assert.IsNotNull(foundInterface);
            }
        }
    }
}