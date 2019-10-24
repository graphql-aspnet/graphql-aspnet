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
    public class FanNoSetter
    {
        public int Blades { get; }

        public string Name { get; set; }
    }
}