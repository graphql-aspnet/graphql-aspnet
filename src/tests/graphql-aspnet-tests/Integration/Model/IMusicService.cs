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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMusicService
    {
        Task<IEnumerable<Artist>> SearchArtists(string searchText);

        Task<IEnumerable<Artist>> RetrieveArtists(params RecordCompany[] companies);

        Task<IEnumerable<Record>> RetrieveRecords(params Artist[] artists);

        Task<IEnumerable<Record>> RetrieveRecords(string searchText, params Artist[] artists);

        Task<IEnumerable<Record>> RetrieveRecords(MusicGenre genre);

        Task<Artist> RetrieveArtist(int artistId);

        Task<MusicGenre> RetrieveGenre(int genreId);

        Task<Artist> CreateArtist(string name, int recordCompanyId);

        Task<Record> CreateRecord(string name, int artistId, int genreId);

        Task<IEnumerable<RecordCompany>> RetrieveRecordCompanies(IEnumerable<int> companyIds);
    }
}