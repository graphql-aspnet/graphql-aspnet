// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.InputValueNodeTestData
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;

    public class Telephone
    {
        [Required]
        [GraphField]
        public int Id { get; set; }

        [GraphField]
        public string Brand { get; set; }
    }
}