// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ThirdPartyDll
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.ThirdPartyDll.Model;

    [GraphRoute("candles")]
    public class CandleController : GraphController
    {
        [Query("candle")]
        public Candle RetrieveCandle(int id)
        {
            return new Candle()
            {
                Id = id,
                Name = "Candle",
                WaxType = WaxType.Beeswax,
            };
        }
    }
}