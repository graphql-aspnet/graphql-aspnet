// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.ConfigurationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class FanController : GraphController
    {
        [QueryRoot("RetrieveFan")]
        public FanItem RetrieveFan(string name)
        {
            return new FanItem()
            {
                Id = 1,
                Name = name,
                FanSpeed = FanSpeed.Medium,
            };
        }
    }
}