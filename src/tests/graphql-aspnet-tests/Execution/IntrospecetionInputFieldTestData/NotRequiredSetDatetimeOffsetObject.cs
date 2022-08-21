// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospecetionInputFieldTestData
{
    using System;

    public class NotRequiredSetDatetimeOffsetObject
    {
        public NotRequiredSetDatetimeOffsetObject()
        {
            this.Property1 = new DateTimeOffset(2022, 8, 20, 11, 59, 0, TimeSpan.Zero);
        }

        public DateTimeOffset Property1 { get; set; }
    }
}