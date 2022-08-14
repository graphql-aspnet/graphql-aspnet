// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphType(InputName = "InputObject")]
    public class InputTestObject
    {
        public InputTestObject()
        {
            this.Id = -1;

            this.TwoProp = new TwoPropertyObject()
            {
                Property1 = "strvalue",
                Property2 = 5,
            };
        }

        public int Id { get; set; }

        public TwoPropertyObject TwoProp { get; set; }
    }
}