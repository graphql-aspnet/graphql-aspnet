// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData
{
    public class WidgetWithDefaultValue
    {
        public WidgetWithDefaultValue()
        {
            this.StringProp = "default 1";
            this.IntProp = 4;
        }

        public string StringProp { get; set; }

        public int IntProp { get; set; }
    }
}
