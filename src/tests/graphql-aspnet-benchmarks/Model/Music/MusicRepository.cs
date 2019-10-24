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

    public class MusicRepository : IMusicRepository
    {
        public MusicRepository()
        {
            var edm = new MusicGenre() { Id = 1, Name = "EDM", };
            var pop = new MusicGenre() { Id = 2, Name = "Pop", };
            var rock = new MusicGenre() { Id = 3, Name = "Rock", };
            var rap = new MusicGenre() { Id = 4, Name = "Rap", };
            this.Genres = new List<MusicGenre>() { edm, pop, rock, rap };

            var sonyMusic = new RecordCompany() { Id = 1, Name = "Sony Music", };
            var emiMusic = new RecordCompany() { Id = 2, Name = "EMI Music", };
            var rbw = new RecordCompany() { Id = 3, Name = "RBW" };
            var sm = new RecordCompany() { Id = 4, Name = "SM" };
            this.Companys = new List<RecordCompany>() { sonyMusic, emiMusic, rbw, sm };

            var mamamoo = new Artist() { Id = 1, Name = "Mamamoo", RecordCompanyId = rbw.Id };
            var jackson = new Artist() { Id = 2, Name = "Michael Jackson", RecordCompanyId = sonyMusic.Id };
            var beyonce = new Artist() { Id = 3, Name = "Beyonce", RecordCompanyId = sonyMusic.Id };
            var alanWalker = new Artist() { Id = 4, Name = "Alan Walker", RecordCompanyId = sonyMusic.Id };
            var pitbull = new Artist() { Id = 5, Name = "Pitbull", RecordCompanyId = sonyMusic.Id };
            var rickyMartin = new Artist() { Id = 6, Name = "Ricky Martin", RecordCompanyId = sonyMusic.Id };
            var queen = new Artist() { Id = 7, Name = "Queen", RecordCompanyId = emiMusic.Id };
            var pinkFloyd = new Artist() { Id = 8, Name = "Pink Floyd", RecordCompanyId = emiMusic.Id };
            var ironMadien = new Artist() { Id = 9, Name = "Iron Madien", RecordCompanyId = emiMusic.Id };
            this.Artists = new List<Artist>() { mamamoo, jackson, beyonce, alanWalker, pitbull, rickyMartin, queen, pinkFloyd, ironMadien };

            var decalcomanie = new Record() { Id = 1, ArtistId = mamamoo.Id, Genre = pop, Name = "Decalcomanie" };
            var gogobebe = new Record() { Id = 2, ArtistId = mamamoo.Id, Genre = pop, Name = "gogobebe" };
            var thriller = new Record() { Id = 3, ArtistId = jackson.Id, Genre = pop, Name = "Thriller" };
            var scream = new Record() { Id = 4, ArtistId = jackson.Id, Genre = pop, Name = "Scream" };
            var singleLadies = new Record() { Id = 5, ArtistId = beyonce.Id, Genre = pop, Name = "Single Ladies" };
            var faded = new Record() { Id = 6, ArtistId = alanWalker.Id, Genre = edm, Name = "Faded" };
            var alone = new Record() { Id = 7, ArtistId = alanWalker.Id, Genre = edm, Name = "Alone" };
            var timber = new Record() { Id = 8, ArtistId = pitbull.Id, Genre = rap, Name = "Timber" };
            var lavidaloca = new Record() { Id = 9, ArtistId = rickyMartin.Id, Genre = pop, Name = "Livin' La Vida Loca" };
            var sheBangs = new Record() { Id = 10, ArtistId = rickyMartin.Id, Genre = pop, Name = "She Bangs" };
            var bohemian = new Record() { Id = 11, ArtistId = queen.Id, Genre = rock, Name = "Bohemian Rhapsody" };
            var champions = new Record() { Id = 12, ArtistId = queen.Id, Genre = rock, Name = "We are the Champions" };
            var shine = new Record() { Id = 13, ArtistId = pinkFloyd.Id, Genre = rock, Name = "Shiny On You Crazy Diamond" };

            this.Records = new List<Record>() { decalcomanie, gogobebe, thriller, scream, singleLadies, faded, alone, timber, lavidaloca, sheBangs, bohemian, champions, shine };
        }

        public List<Artist> Artists { get; }

        public List<Record> Records { get; }

        public List<MusicGenre> Genres { get; }

        public List<RecordCompany> Companys { get; }
    }
}