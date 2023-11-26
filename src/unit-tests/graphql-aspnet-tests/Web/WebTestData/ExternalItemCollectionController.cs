// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Web.WebTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ExternalItemCollectionController : GraphController
    {
        [QueryRoot]
        public int ItemPassed()
        {
            if (this.Request.Items != null && this.Request.Items.ContainsKey("test-key") && this.Request.Items["test-key"] is TwoPropertyObject item)
            {
                return item.Property2;
            }

            return -1;
        }
    }
}