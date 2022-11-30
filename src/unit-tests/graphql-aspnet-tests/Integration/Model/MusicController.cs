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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphRoute("music")]
    public class MusicController : GraphController
    {
        private readonly IMusicService _musicService;

        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        [QueryRoot("artists", typeof(IEnumerable<Artist>))]
        public async Task<IGraphActionResult> SearchArtists(string searchText)
        {
            var artists = await _musicService.SearchArtists(searchText);
            return this.Ok(artists);
        }

        [Mutation("createArtist", typeof(Artist))]
        public async Task<IGraphActionResult> CreateArtist(string artistName, int recordCompanyId)
        {
            try
            {
                var arist = await _musicService.CreateArtist(artistName, recordCompanyId);
                if (arist == null)
                    throw new ArgumentException("An unknown error occured generating the artist");

                return this.Ok(arist);
            }
            catch (ArgumentException ex)
            {
                return this.Error(ex.Message);
            }
            catch (Exception e)
            {
                return this.Error("An unknown error occured", Constants.ErrorCodes.UNHANDLED_EXCEPTION, e);
            }
        }

        [Mutation("createRecord", typeof(Record))]
        public async Task<IGraphActionResult> CreateRecord(Artist artist, MusicGenre genre, string songName)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);

            var song = await _musicService.CreateRecord(songName, artist.Id, genre.Id);
            return this.Ok(song);
        }

        [BatchTypeExtension(typeof(Artist), "records", typeof(Record), TypeExpression = TypeExpressions.IsList)]
        public async Task<IGraphActionResult> RetrieveArtistRecords(IEnumerable<Artist> artists, string searchText = null)
        {
            var songs = await _musicService.RetrieveRecords(searchText, artists.ToArray());
            return this.StartBatch()
                .FromSource(artists, artist => artist.Id)
                .WithResults(songs, song => song.ArtistId)
                .Complete();
        }

        [BatchTypeExtension(typeof(Artist), "company", typeof(RecordCompany))]
        public async Task<IGraphActionResult> RetrieveArtistCompany(IEnumerable<Artist> artists)
        {
            var companies = await _musicService.RetrieveRecordCompanies(artists.Select(x => x.RecordCompanyId));
            return this.StartBatch()
                .FromSource(artists, artist => artist.RecordCompanyId)
                .WithResults(companies, company => company.Id)
                .Complete();
        }
    }
}