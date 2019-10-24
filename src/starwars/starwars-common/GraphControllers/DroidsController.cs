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
    /// A controller for operations dealing directly with humans.
    /// </summary>
    public class DroidsController : GraphController
    {
        private readonly IStarWarsDataService _starWarsData;

        /// <summary>
        /// Initializes a new instance of the <see cref="DroidsController" /> class.
        /// </summary>
        /// <param name="starWarsData">The star wars data.</param>
        public DroidsController(IStarWarsDataService starWarsData)
        {
            // NOTE: This service is scoped to the request, just like a normal DI request in asp.net core.
            // the underlying repository is a singleton for app instance (see Startup.cs)
            _starWarsData = Validation.ThrowIfNullOrReturn(starWarsData, nameof(starWarsData));
        }

        /// <summary>
        /// Retrieves all the droids in the data respnsitory that match the search text.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [Query("search", typeof(IEnumerable<Droid>))]
        [Description("Retrieves all the droids in the data respository that match the search text (not case sensitive).")]
        public async Task<IGraphActionResult> Droids(string searchText = "*")
        {
            var starships = await _starWarsData.SearchHumans(searchText).ConfigureAwait(false);
            return this.Ok(starships);
        }

        /// <summary>
        /// Retrieves a droid in the system by their id. Note the use of a different name for the parameter
        /// between its exposure in the object graph vs. the formal parameter name used in the C# code.
        /// </summary>
        /// <param name="droidId">The droid identifier.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [QueryRoot("droid", typeof(Droid))]
        [Description("Retrieves a droid in the system by their id.")]
        public async Task<IGraphActionResult> RetrieveDroid([FromGraphQL("id")] GraphId droidId)
        {
            var droid = await _starWarsData.RetrieveDroid(droidId).ConfigureAwait(false);
            return this.Ok(droid);
        }
    }
}