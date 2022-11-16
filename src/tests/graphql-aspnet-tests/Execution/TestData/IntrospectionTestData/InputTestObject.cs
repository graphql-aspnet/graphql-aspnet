// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphType(InputName = "InputObject")]
    [Description("input obj desc")]
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

        [Description("not required but set int")]
        public int NotRequiredButSetId { get; set; }

        [Required]
        [Description("required int")]
        public int RequiredId { get; set; }

        [Required]
        [Description("required bool")]
        public bool RequiredBool { get; set; }

        [Description("unrequired but true bool")]
        public bool UnrequiredButTrueBool { get; set; }

        [Description("two prop with default value")]
        public TwoPropertyObject TwoPropWithDefaultValue { get; set; }

        [Description("two prop no default value")]
        public TwoPropertyObject TwoPropWithNoDefaultValue { get; set; }

        [Required]
        [Description("required two prop")]
        public TwoPropertyObject RequiredTwoProp { get; set; }
    }
}