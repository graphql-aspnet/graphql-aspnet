// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class InputTestObjectWithDefaultFieldValues
    {
        public InputTestObjectWithDefaultFieldValues()
        {
            this.Prop1 = 34;
            this.Prop2 = "default prop2 string";
            this.Prop4 = new TwoPropertyObject("twoPropString1", 99);
        }

        public int Prop1 { get; set; }

        public string Prop2 { get; set; }

        [Required]
        public int Prop3 { get; set; }

        public TwoPropertyObject Prop4 { get; set; }
    }
}