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
    using GraphQL.AspNet.Attributes;

    public class Record
    {
        [GraphField]
        public int Id { get; set; }

        [GraphField]
        public int ArtistId { get; set; }

        [GraphField]
        public MusicGenre Genre { get; set; }

        [GraphField]
        public string Name { get; set; }
    }
}