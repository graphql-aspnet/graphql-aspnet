// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ThirdPartyDll.Model
{
    using GraphQL.AspNet.Attributes;

    [GraphType]
    public class Candle
    {
        [GraphField]
        public int Id { get; set; }

        [GraphField]
        public string Name { get; set; }

        [GraphField]
        public WaxType WaxType { get; set; }
    }
}