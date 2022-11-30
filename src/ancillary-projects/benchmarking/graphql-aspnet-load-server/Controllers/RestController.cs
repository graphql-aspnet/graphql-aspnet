// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.Server.Controllers
{
    using GraphQL.AspNet.SubscriberLoadTest.Models.Models.ClientModels;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A sample rest controller to profile against.
    /// </summary>
    [ApiController]
    public class RestController : Controller
    {
        private static readonly Donut _donut;

        static RestController()
        {
            _donut = new Donut()
            {
                Id = "5",
                Name = "Static Rest Donut",
                Flavor = "vanilla",
            };
        }

        /// <summary>
        /// GET - /api/donuts/{id}.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Donut.</returns>
        [HttpGet("/api/donuts/{id}")]
        public IActionResult RetrieveDonut(string id)
        {
            return this.Ok(_donut);
        }
    }
}