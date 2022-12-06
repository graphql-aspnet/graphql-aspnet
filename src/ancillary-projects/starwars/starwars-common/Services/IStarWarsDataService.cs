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
    using System.Threading.Tasks;
    using GraphQL.AspNet.StarwarsAPI.Common.Model;

    /// <summary>
    /// A data service for adding or retrieving data items related to the star wars universe.
    /// </summary>
    public interface IStarWarsDataService
    {
        /// <summary>
        /// Retrieves a single human with the given id.
        /// </summary>
        /// <param name="humanId">The human identifier.</param>
        /// <returns>Task&lt;Human&gt;.</returns>
        Task<Human> RetrieveHuman(GraphId humanId);

        /// <summary>
        /// Searches the humans lookign for any names matching the supplied text. Use "*" to retrieve all
        /// humans.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>Task&lt;IEnumerable&lt;Human&gt;&gt;.</returns>
        Task<IEnumerable<Human>> SearchHumans(string searchText);

        /// <summary>
        /// Retrieves a single droid with the given id.
        /// </summary>
        /// <param name="droidId">The droid identifier.</param>
        /// <returns>Task&lt;Droid&gt;.</returns>
        Task<Droid> RetrieveDroid(GraphId droidId);

        /// <summary>
        /// Searches the droids looking for any names matching the supplied text. Use "*" to retrieve all
        /// humans.
        /// </summary>
        /// <param name="searchText">The search text to match on.</param>
        /// <returns>Task&lt;IEnumerable&lt;Droid&gt;&gt;.</returns>
        Task<IEnumerable<Droid>> SearchDroids(string searchText);

        /// <summary>
        /// Retreives the character with the given id. Returns null if no character found.
        /// </summary>
        /// <param name="characterId">The character identifier.</param>
        /// <returns>Task&lt;ICharacter&gt;.</returns>
        Task<ICharacter> RetreiveCharacter(GraphId characterId);

        /// <summary>
        /// Retrieves a set of characters with the supplied ids.
        /// </summary>
        /// <param name="characterIds">The character ids to retrieve.</param>
        /// <returns>Task&lt;IEnumerable&lt;ICharacter&gt;&gt;.</returns>
        Task<IEnumerable<ICharacter>> RetrieveCharacters(IEnumerable<GraphId> characterIds);

        /// <summary>
        /// Searches the characters names looking for any matching text. Use "*" to retrieve all
        /// characters.
        /// </summary>
        /// <param name="searchText">The search text to match on.</param>
        /// <returns>Task&lt;IEnumerable&lt;ICharacter&gt;&gt;.</returns>
        Task<IEnumerable<ICharacter>> SearchCharacters(string searchText);

        /// <summary>
        /// Retrieves a single starship by its id. Returns null if no starship is found.
        /// </summary>
        /// <param name="starshipId">The starship identifier.</param>
        /// <returns>Task&lt;Starship&gt;.</returns>
        Task<Starship> RetrieveStarship(GraphId starshipId);

        /// <summary>
        /// Searches the starships names looking for any matching text. Use "*" to retrieve all
        /// ships.
        /// </summary>
        /// <param name="searchText">The search text to match on.</param>
        /// <returns>Task&lt;Starship&gt;.</returns>
        Task<IEnumerable<Starship>> SearchStarships(string searchText);
    }
}