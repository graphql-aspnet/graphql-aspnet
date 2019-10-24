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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.StarwarsAPI.Common.Model;

    /// <summary>
    /// A data service for adding or retrieving data items related to the star wars universe. This class implements
    /// its methods as Tasks for sake of completeness as this would be the normal approach if using an ORM or other data
    /// persistance method within a service class.
    /// </summary>
    public class StarWarsDataService : IStarWarsDataService
    {
        /// <summary>
        /// A constnat indicating to the data service to return all found items and not perform any filtering.
        /// </summary>
        public const string WILD_CARD = "*";

        private readonly StarWarsDataRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="StarWarsDataService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public StarWarsDataService(StarWarsDataRepository repository)
        {
            _repository = Validation.ThrowIfNullOrReturn(repository, nameof(repository));
        }

        /// <summary>
        /// Retreives the character with the given id. Returns null if no character found.
        /// </summary>
        /// <param name="characterId">The character identifier.</param>
        /// <returns>Task&lt;ICharacter&gt;.</returns>
        public Task<ICharacter> RetreiveCharacter(GraphId characterId)
        {
            return _repository.Characters.FirstOrDefault(x => x.Id == characterId).AsCompletedTask();
        }

        /// <summary>
        /// Retrieves a single human with the given id.
        /// </summary>
        /// <param name="humanId">The human identifier.</param>
        /// <returns>Task&lt;Human&gt;.</returns>
        public Task<Human> RetrieveHuman(GraphId humanId)
        {
            return _repository.Characters.OfType<Human>().FirstOrDefault(x => x.Id == humanId).AsCompletedTask();
        }

        /// <summary>
        /// Retrieves a single droid with the given id.
        /// </summary>
        /// <param name="droidId">The droid identifier.</param>
        /// <returns>Task&lt;Droid&gt;.</returns>
        public Task<Droid> RetrieveDroid(GraphId droidId)
        {
            return _repository.Characters.OfType<Droid>().FirstOrDefault(x => x.Id == droidId).AsCompletedTask();
        }

        /// <summary>
        /// Retrieves a set of characters with the supplied ids.
        /// </summary>
        /// <param name="characterIds">The character ids to retrieve.</param>
        /// <returns>Task&lt;IEnumerable&lt;ICharacter&gt;&gt;.</returns>
        public Task<IEnumerable<ICharacter>> RetrieveCharacters(IEnumerable<GraphId> characterIds)
        {
            characterIds = characterIds ?? Enumerable.Empty<GraphId>();
            return _repository.Characters.Where(x => characterIds.Contains(x.Id)).AsCompletedTask();
        }

        /// <summary>
        /// Searches the characters names looking for any matching text. Use "*" to retrieve all
        /// characters.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Task&lt;IEnumerable&lt;ICharacter&gt;&gt;.</returns>
        public Task<IEnumerable<ICharacter>> SearchCharacters(string text = WILD_CARD)
        {
            text = text?.Trim().ToLower() ?? WILD_CARD;
            if (text == WILD_CARD)
                return ((IEnumerable<ICharacter>)_repository.Characters).AsCompletedTask();

            return _repository.Characters.Where(x => x.Name.ToLower().Contains(text)).AsCompletedTask();
        }

        /// <summary>
        /// Searches the starships names looking for any matching text. Use "*" to retrieve all
        /// ships.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Task&lt;Starship&gt;.</returns>
        public Task<IEnumerable<Starship>> SearchStarships(string text = WILD_CARD)
        {
            text = text?.Trim().ToLower() ?? WILD_CARD;
            if (text == WILD_CARD)
                return ((IEnumerable<Starship>)_repository.Starships).AsCompletedTask();

            return _repository.Starships.Where(x => x.Name.ToLower().Contains(text)).AsCompletedTask();
        }

        /// <summary>
        /// Retrieves a single starship by its id. Returns null if no starship is found.
        /// </summary>
        /// <param name="starshipId">The starship identifier.</param>
        /// <returns>Task&lt;Starship&gt;.</returns>
        public Task<Starship> RetrieveStarship(GraphId starshipId)
        {
            return _repository.Starships.FirstOrDefault(x => x.Id == starshipId).AsCompletedTask();
        }

        /// <summary>
        /// Searches the humans lookign for any names matching the supplied text. Use "*" to retrieve all
        /// humans.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>Task&lt;IEnumerable&lt;Human&gt;&gt;.</returns>
        public Task<IEnumerable<Human>> SearchHumans(string searchText)
        {
            searchText = searchText?.Trim().ToLower() ?? WILD_CARD;
            if (searchText == WILD_CARD)
                return Task.FromResult(_repository.Characters.OfType<Human>());

            return _repository.Characters.OfType<Human>().Where(x => x.Name.ToLower().Contains(searchText)).AsCompletedTask();
        }

        /// <summary>
        /// Searches the droids looking for any names matching the supplied text. Use "*" to retrieve all
        /// humans.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>Task&lt;IEnumerable&lt;Droid&gt;&gt;.</returns>
        public Task<IEnumerable<Droid>> SearchDroids(string searchText)
        {
            searchText = searchText?.Trim().ToLower() ?? WILD_CARD;
            if (searchText == WILD_CARD)
                return Task.FromResult(_repository.Characters.OfType<Droid>());

            return _repository.Characters.OfType<Droid>().Where(x => x.Name.ToLower().Contains(searchText)).AsCompletedTask();
        }
    }
}