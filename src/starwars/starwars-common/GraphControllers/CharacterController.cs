// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.StarwarsAPI.Common.GraphControllers
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.StarwarsAPI.Common.Model;
    using GraphQL.AspNet.StarwarsAPI.Common.Services;

    /// <summary>
    /// Responsible for character related graph operation.
    /// </summary>
    [GraphRoute("characters")]
    public class CharacterController : GraphController
    {
        private readonly IStarWarsDataService _starWarsData;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterController"/> class.
        /// </summary>
        /// <param name="starWarsData">The star wars data.</param>
        public CharacterController(IStarWarsDataService starWarsData)
        {
            // NOTE: This service is scoped to the request, just like a normal DI request in asp.net core.
            // the underlying repository is a singleton for app instance (see Startup.cs)
            _starWarsData = Validation.ThrowIfNullOrReturn(starWarsData, nameof(starWarsData));
        }

        /// <summary>
        /// Retrieves the hero of any given movie.
        /// </summary>
        /// <param name="episode">The episode.</param>
        /// <returns>Task&lt;IActionResult&gt;.</returns>
        [PossibleTypes(typeof(Human), typeof(Droid))]
        [QueryRoot("hero", typeof(ICharacter), TypeExpression = TypeExpressions.IsNotNull)]
        public async Task<IGraphActionResult> RetrieveHero(MovieEpisode episode)
        {
            ICharacter hero = null;

            // luke is the hero of empire (and our hearts) but R2 is the hero otherwise.
            if (episode == MovieEpisode.Empire)
                hero = await _starWarsData.RetreiveCharacter(StarWarsConstants.LukeSkywalkerId).ConfigureAwait(false);
            else
                hero = await _starWarsData.RetreiveCharacter(StarWarsConstants.R2D2Id).ConfigureAwait(false);

            return this.Ok(hero);
        }

        /// <summary>
        /// Retrieves any given character in the system.
        /// </summary>
        /// <param name="id">The id of the character.</param>
        /// <returns>Task&lt;IActionResult&gt;.</returns>
        [PossibleTypes(typeof(Human), typeof(Droid))]
        [QueryRoot("character", typeof(ICharacter))]
        public async Task<IGraphActionResult> RetrieveCharacter(GraphId id)
        {
            var character = await _starWarsData.RetreiveCharacter(id).ConfigureAwait(false);
            return this.Ok(character);
        }

        /// <summary>
        /// Retrieves all the friends from all the supplied characters in batch and returns them
        /// to be projected into the graph for any <see cref="ICharacter" /> object that was provided.
        /// </summary>
        /// <param name="characters">The characters to retrieve friends for.</param>
        /// <returns>IEnumerable&lt;ICharacter&gt;.</returns>
        [Description("This character's friends, or an empty list if they have none")]
        [BatchTypeExtension(typeof(ICharacter), "friends", typeof(ICharacter), TypeExpression = TypeExpressions.IsList)]
        public async Task<IGraphActionResult> RetrieveACharactersFriends(IEnumerable<ICharacter> characters)
        {
            var allFriends = await _starWarsData.RetrieveCharacters(characters.SelectMany(x => x.FriendIds)).ConfigureAwait(false);

            return this.StartBatch()
                .FromSource(characters, c => c.Id)
                .WithResults(allFriends, friend => friend.FriendIds)
                .Complete();
        }
    }
}