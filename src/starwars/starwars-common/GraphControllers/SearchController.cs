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
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.StarwarsAPI.Common.Model;
    using GraphQL.AspNet.StarwarsAPI.Common.Services;

    /// <summary>
    /// A controller to handle search queries across the spectrum of the star wars universe.
    /// </summary>
    public class SearchController : GraphController
    {
        private readonly IStarWarsDataService _starWarsData;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class.
        /// </summary>
        /// <param name="starWarsData">The star wars data.</param>
        public SearchController(IStarWarsDataService starWarsData)
        {
            _starWarsData = starWarsData;
        }

        /// <summary>
        /// Searches for the specified text as the name of a starship or star wars character.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        /// <returns>Task&lt;IGraphActionResult&gt;.</returns>
        [QueryRoot("search", "SearchResults", typeof(Droid), typeof(Human), typeof(Starship), TypeExpression = TypeExpressions.IsList)]
        [Description("Searches for the specified text as the name of a starship or or character (not case sensitive).")]
        public async Task<IGraphActionResult> GlobalSearch(string searchText = "*")
        {
            var characters = await _starWarsData.SearchCharacters(searchText).ConfigureAwait(false);
            var ships = await _starWarsData.SearchStarships(searchText).ConfigureAwait(false);

            var data = characters.Cast<object>().Concat(ships).ToList();
            return this.Ok(data);
        }
    }
}