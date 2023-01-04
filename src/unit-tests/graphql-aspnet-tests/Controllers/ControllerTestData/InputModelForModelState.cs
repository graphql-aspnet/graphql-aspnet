// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.ControllerTestData
{
    using System.ComponentModel.DataAnnotations;

    public class InputModelForModelState
    {
        [StringLength(5)]
        public string Name { get; set; }
    }
}