// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;

    public class SimplePropertyObject
    {
        public class HairData
        {
            public string HairColor { get; set; }
        }

        public string Name { get; }

        [GraphField("SuperAge")]
        public int Age { get; }

        [Description("A Prop Description")]
        public string Address1 { get; }

        [Deprecated("A Reason")]
        public string Address2 { get; }

        [PropAuthorize]
        public string LastName { get; }

        [GraphField("City!")]
        public string City { get; }

        [GraphSkip]
        public string State { get; }

        public HairData Hair { get; set; }

        public List<HairData> Wigs { get; set; }
    }
}