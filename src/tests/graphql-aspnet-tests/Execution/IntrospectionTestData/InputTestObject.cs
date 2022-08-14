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
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphType(InputName = "InputObject")]
    public class InputTestObject
    {
        public InputTestObject()
        {
            this.NotRequiredButSetId = -1;

            this.TwoPropWithDefaultValue = new TwoPropertyObject()
            {
                Property1 = "strvalue",
                Property2 = 5,
            };

            this.UnrequiredButTrueBool = true;
        }

        public int NotRequiredButSetId { get; set; }

        [Required]
        public int RequiredId { get; set; }

        [Required]
        public bool RequiredBool { get; set; }

        public bool UnrequiredButTrueBool { get; set; }

        public TwoPropertyObject TwoPropWithDefaultValue { get; set; }

        public TwoPropertyObject TwoPropWithNoDefaultValue { get; set; }

        [Required]
        public TwoPropertyObject RequiredTwoProp { get; set; }
    }
}