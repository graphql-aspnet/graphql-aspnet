// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.DictioanryConversionData
{
    public class FanPlug
    {
        public int PlugId { get; set; }

        public Fan AttachedFan { get; set; }
    }
}