// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class ThingController : GraphController
    {
        [QueryRoot("Thing")]
        public Thing ReturnData()
        {
            return new Thing();
        }

        [Query("moreData")]
        public int MoreData()
        {
            return 0;
        }
    }
}