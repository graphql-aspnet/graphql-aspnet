// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.StarwarsAPI.Common.Services
{
    using System.Collections.Generic;
    using GraphQL.AspNet.StarwarsAPI.Common.Model;

    /// <summary>
    /// An in-memory data storage mechanism for star wars data to facilitate this example. Ideally this data would be persisted to a databasel.
    /// </summary>
    public class StarWarsDataRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarWarsDataRepository"/> class.
        /// </summary>
        public StarWarsDataRepository()
        {
            this.Characters = new List<ICharacter>();
            this.Starships = new List<Starship>();

            this.PopulateCharacters();
            this.PopulateStarships();
        }

        /// <summary>
        /// Populates the characters to the master list.
        /// </summary>
        private void PopulateCharacters()
        {
            // people
            this.Characters.Add(new Human(StarWarsConstants.LukeSkywalkerId)
            {
                Name = "Luke Skywalker",
                FriendIds = new List<GraphId>() { (GraphId)"1002", (GraphId)"1003", (GraphId)"2000", StarWarsConstants.R2D2Id },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope, MovieEpisode.Empire, MovieEpisode.Jedi },
                HomePlanet = "Tatooine",
                HeightInMeters = 1.72f,
                Mass = 77,
                StarshipIds = new List<GraphId>() { (GraphId)"3001", (GraphId)"3003" },
            });

            this.Characters.Add(new Human("1001")
            {
                Name = "Darth Vader",
                FriendIds = new List<GraphId>() { (GraphId)"1004" },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope, MovieEpisode.Empire, MovieEpisode.Jedi },
                HomePlanet = "Tatooine",
                HeightInMeters = 2.02f,
                Mass = 136,
                StarshipIds = new List<GraphId>() { (GraphId)"3002" },
            });

            this.Characters.Add(new Human("1002")
            {
                Name = "Han Solo",
                FriendIds = new List<GraphId>() { StarWarsConstants.LukeSkywalkerId, (GraphId)"1003", StarWarsConstants.R2D2Id },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope, MovieEpisode.Empire, MovieEpisode.Jedi },
                HomePlanet = null,
                HeightInMeters = 1.8f,
                Mass = 80,
                StarshipIds = new List<GraphId>() { (GraphId)"3000", (GraphId)"3003" },
            });

            this.Characters.Add(new Human("1003")
            {
                Name = "Leia Organa",
                FriendIds = new List<GraphId>() { StarWarsConstants.LukeSkywalkerId, (GraphId)"1002", (GraphId)"2000", StarWarsConstants.R2D2Id },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope, MovieEpisode.Empire, MovieEpisode.Jedi },
                HomePlanet = "Alderaan",
                HeightInMeters = 1.5f,
                Mass = 49,
                StarshipIds = new List<GraphId>(),
            });

            this.Characters.Add(new Human("1004")
            {
                Name = "Wilhuff Tarkin",
                FriendIds = new List<GraphId>() { (GraphId)"1001" },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope },
                HeightInMeters = 1.8f,
                Mass = null,
                StarshipIds = new List<GraphId>(),
            });

            // droids
            this.Characters.Add(new Droid("2000")
            {
                Name = "C-3PO",
                FriendIds = new List<GraphId>() { StarWarsConstants.LukeSkywalkerId, (GraphId)"1002", (GraphId)"1003", StarWarsConstants.R2D2Id },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope, MovieEpisode.Empire, MovieEpisode.Jedi },
                PrimaryFunction = "Protocol",
            });

            this.Characters.Add(new Droid(StarWarsConstants.R2D2Id)
            {
                Name = "R2-D2",
                FriendIds = new List<GraphId>() { StarWarsConstants.LukeSkywalkerId, (GraphId)"1002", (GraphId)"1003" },
                AppearsIn = new List<MovieEpisode>() { MovieEpisode.NewHope, MovieEpisode.Empire, MovieEpisode.Jedi },
                PrimaryFunction = "Astromech",
            });
        }

        /// <summary>
        /// Populates the starships to the master list.
        /// </summary>
        private void PopulateStarships()
        {
            this.Starships.Add(new Starship("3000")
            {
                Name = "Millenium Falcon",
                LengthInMeters = 34.37f,
                Coordinates = new List<IEnumerable<float>>
                {
                    new List<float>() { 5, 5.12f, 8.23f, },
                    new List<float>() { 3.14f, 3.15f, 3.16f, },
                    new List<float>() { 7, 8, 9, },
                },
            });

            this.Starships.Add(new Starship("3001")
            {
                Name = "X-Wing",
                LengthInMeters = 12.5f,
            });

            this.Starships.Add(new Starship("3002")
            {
                Name = "TIE Advanced x1",
                LengthInMeters = 9.2f,
            });

            this.Starships.Add(new Starship("3003")
            {
                Name = "Imperial shuttle",
                LengthInMeters = 20f,
            });
        }

        /// <summary>
        /// Gets the characters of the star wars universe.
        /// </summary>
        /// <value>The characters.</value>
        public List<ICharacter> Characters { get; }

        /// <summary>
        /// Gets the starships known to this repo.
        /// </summary>
        /// <value>The starships.</value>
        public List<Starship> Starships { get; }
    }
}