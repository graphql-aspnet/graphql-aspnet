// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ActionTestData
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class OneMethodSubscriptionController : GraphController
    {
        [Subscription("path1")]
        [Description("SubDescription")]
        public TwoPropertyObject SingleMethod(TwoPropertyObject data)
        {
            return data;
        }
    }
}