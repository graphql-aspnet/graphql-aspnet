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
    using System.Collections.Generic;

    public class FanWall
    {
        public FanWall()
        {
            this.Fans = new List<Fan>();
        }

        public int WallId { get; set; }

        public List<Fan> Fans { get; set; }
    }
}