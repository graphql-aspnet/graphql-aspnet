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
    using System.Collections.Generic;

    public class NotRequiredSetListIntObject
    {
        public NotRequiredSetListIntObject()
        {
            this.Property1 = new List<int>() { 1, 2, 3, 4 };
        }

        public List<int> Property1 { get; set; }
    }
}