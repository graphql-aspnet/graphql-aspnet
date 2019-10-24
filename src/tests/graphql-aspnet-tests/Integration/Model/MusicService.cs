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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;

    public class MusicService : IMusicService
    {
        private readonly IMusicRepository _repository;

        public MusicService(IMusicRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Artist>> SearchArtists(string searchText)
        {
            searchText = searchText?.Trim() ?? "*";
            if (searchText == "*")
                return ((IEnumerable<Artist>)_repository.Artists).AsCompletedTask();

            return _repository.Artists.Where(x => x.Name.ToLower().Contains(searchText)).AsCompletedTask();
        }

        public Task<IEnumerable<Artist>> RetrieveArtists(params RecordCompany[] companies)
        {
            if (companies == null || !companies.Any())
                return Enumerable.Empty<Artist>().AsCompletedTask();

            return _repository.Artists.Where(x => companies.Any(y => y.Id == x.RecordCompanyId)).AsCompletedTask();
        }

        public Task<IEnumerable<Record>> RetrieveRecords(params Artist[] artists)
        {
            if (artists == null)
                return Enumerable.Empty<Record>().AsCompletedTask();

            return _repository.Records.Where(x => artists.Any(y => y.Id == x.ArtistId)).AsCompletedTask();
        }

        public Task<IEnumerable<Record>> RetrieveRecords(string searchText, params Artist[] artists)
        {
            artists = artists ?? new Artist[0];

            var records = artists.Length == 0 ? _repository.Records : _repository.Records.Where(x => artists.Any(y => y.Id == x.ArtistId));

            searchText = searchText?.ToLower().Trim() ?? "*";

            if (searchText != "*")
                records = records.Where(x => x.Name.ToLower().Contains(searchText));

            return records.AsCompletedTask();
        }

        public Task<IEnumerable<Record>> RetrieveRecords(MusicGenre genre)
        {
            if (genre == null)
                return Enumerable.Empty<Record>().AsCompletedTask();

            return _repository.Records.Where(x => x.Genre.Id == genre.Id).AsCompletedTask();
        }

        public Task<Artist> RetrieveArtist(int artistId)
        {
            return _repository.Artists.FirstOrDefault(x => x.Id == artistId).AsCompletedTask();
        }

        public Task<MusicGenre> RetrieveGenre(int genreId)
        {
            return _repository.Genres.FirstOrDefault(x => x.Id == genreId).AsCompletedTask();
        }

        public Task<Artist> CreateArtist(string name, int recordCompanyId)
        {
            var company = _repository.Companys.FirstOrDefault(x => x.Id == recordCompanyId);
            if (company == null)
                throw new ArgumentException($"Record company with id '{recordCompanyId}' not found.");

            var artist = new Artist()
            {
                Id = _repository.Artists.Max(x => x.Id) + 1,
                RecordCompanyId = company.Id,
                Name = name,
            };
            _repository.Artists.Add(artist);

            return artist.AsCompletedTask();
        }

        public Task<Record> CreateRecord(string name, int artistId, int genreId)
        {
            Validation.ThrowIfNull(name, nameof(name));
            var genre = _repository.Genres.FirstOrDefault(x => x.Id == genreId);
            if (genre == null)
                throw new ArgumentException($"Music Genre with id '{genreId}' not found.");

            var artist = _repository.Artists.FirstOrDefault(x => x.Id == artistId);
            if (artist == null)
                throw new ArgumentException($"Artist with id '{artistId}' not found.");

            var record = new Record()
            {
                ArtistId = artist.Id,
                Genre = genre,
                Id = _repository.Records.Max(x => x.Id) + 1,
                Name = name.Trim(),
            };

            _repository.Records.Add(record);
            return record.AsCompletedTask();
        }

        public Task<IEnumerable<RecordCompany>> RetrieveRecordCompanies(IEnumerable<int> companyIds)
        {
            companyIds = companyIds ?? Enumerable.Empty<int>();
            var list = companyIds.ToList();
            return _repository.Companys.Where(x => list.Contains(x.Id)).AsCompletedTask();
        }
    }
}