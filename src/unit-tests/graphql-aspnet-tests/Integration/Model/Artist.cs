// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Integration.Model
{
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;

    public class Artist
    {
        [GraphField]
        [Required]
        public int Id { get; set; }

        [GraphField]
        [Required]
        public int RecordCompanyId { get; set; }

        [GraphField]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}