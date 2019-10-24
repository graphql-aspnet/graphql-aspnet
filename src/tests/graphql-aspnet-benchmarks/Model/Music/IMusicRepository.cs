// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Benchmarks.Model
{
    using System.Collections.Generic;

    public interface IMusicRepository
    {
        List<Artist> Artists { get; }

        List<Record> Records { get; }

        List<MusicGenre> Genres { get; }

        List<RecordCompany> Companys { get; }
    }
}