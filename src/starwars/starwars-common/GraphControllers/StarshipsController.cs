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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.StarwarsAPI.Common.Model;
    using GraphQL.AspNet.StarwarsAPI.Common.Services;

    /// <summary>
    /// A graph controller responsible for managing starship related data.
    /// </summary>
    public class StarshipsController : GraphController
    {
        private readonly IStarWarsDataService _starWarsData;

        /// <summary>
        /// Initializes a new instance of the <see cref="StarshipsController"/> class.
        /// </summary>
        /// <param name="starWarsData">The star wars data.</param>
        public StarshipsController(IStarWarsDataService starWarsData)
        {
            // NOTE: This service is scoped to the request, just like a normal DI request in asp.net core.
            // the underlying repository is a singleton for app instance (see Startup.cs)
            _starWarsData = Validation.ThrowIfNullOrReturn(starWarsData, nameof(starWarsData));
        }

        /// <summary>
        /// Retrieves all the starships in the data responsitory.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [Query("search", typeof(IEnumerable<Starship>))]
        [Description("Retrieves all the starships in the data respository that match the search text (not case sensitive).")]
        public async Task<IGraphActionResult> SearchStarships(string searchText = "*")
        {
            var starships = await _starWarsData.SearchStarships(searchText).ConfigureAwait(false);
            return this.Ok(starships);
        }

        /// <summary>
        /// Retrieves a starship in the system by its id. Note the use of a different name for the parameter
        /// between its exposure in the object graph vs. the formal parameter name used in the C# code.
        /// </summary>
        /// <param name="starshipId">The starship identifier.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [QueryRoot("starship", typeof(Starship))]
        [Description("Retrieves a single starship by its given id.")]
        public async Task<IGraphActionResult> RetrieveStarship([FromGraphQL("id")] GraphId starshipId)
        {
            var starship = await _starWarsData.RetrieveStarship(starshipId).ConfigureAwait(false);
            return this.Ok(starship);
        }


        /// <summary>
        /// Retrieves a droid in the system by their id. Note the use of a different name for the parameter
        /// between its exposure in the object graph vs. the formal parameter name used in the C# code.
        /// </summary>
        /// <param name="starship">The data package used to update a starship.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [Mutation("updateStarship", typeof(Starship))]
        [Description("Retrieves a single starship by its given id.")]
        public async Task<IGraphActionResult> UpdateStarship(StarshipUpdateModel starship)
        {
            if (!ModelState.IsValid)
                return this.BadRequest(this.ModelState);

            var existingStarship = await _starWarsData.RetrieveStarship(starship.Id).ConfigureAwait(false);
            if (existingStarship == null)
                return this.BadRequest($"Starship with id {starship.Id} was not found.");

            // update the model object from the repository
            existingStarship.Name = starship.Name;

            // raise an event for any listening subscriptions
            this.PublishSubscriptionEvent("starshipUpdated", existingStarship);
            return this.Ok(existingStarship);
        }

        /// <summary>
        /// A subscription to which any altered starship is sent whenever any
        /// user makes a change to it. Starships that are deleted are not communicated through this subscription.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="nameLike">A name filter, supplied by the subscriber to determine which
        /// ships they actually recieve.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [SubscriptionRoot("starship", typeof(Starship), EventName = "starshipUpdated")]
        [Description("Returns an updated starship anytime it is altered.")]
        public Task<IGraphActionResult> StarshipUpdated(Starship eventData, string nameLike = "*")
        {
            if (eventData != null && (nameLike == "*" || eventData.Name.Contains(nameLike)))
                return Task.FromResult(this.Ok(eventData));

            return Task.FromResult(this.Ok());
        }
    }
}