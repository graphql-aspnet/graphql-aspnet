// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer.ClientSubscriptionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ClientSubscriptionTestController : GraphController
    {
        [SubscriptionRoot]
        public TwoPropertyObject WatchObjects(TwoPropertyObject data, string propLike = "*")
        {
            if (propLike == "*")
                return data;

            if (data.Property1.Contains(propLike))
                return data;

            return null;
        }

        [QueryRoot]
        public TwoPropertyObject RetrieveObject(int id)
        {
            return null;
        }
    }
}