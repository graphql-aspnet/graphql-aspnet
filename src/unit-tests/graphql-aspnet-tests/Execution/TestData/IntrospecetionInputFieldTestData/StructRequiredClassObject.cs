// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospecetionInputFieldTestData
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public struct StructRequiredClassObject
    {
        [Required]
        public TwoPropertyObject Property1 { get; set; }
    }
}