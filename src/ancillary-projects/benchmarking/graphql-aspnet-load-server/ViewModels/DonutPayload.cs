// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1600 // Elements should be documented
namespace GraphQL.AspNet.SubscriberLoadTest.Server
{
    using GraphQL.AspNet.SubscriberLoadTest.Models.Models.ClientModels;

    public class DonutPayload
    {
        public Donut SingleDonut { get; set; }
    }
}