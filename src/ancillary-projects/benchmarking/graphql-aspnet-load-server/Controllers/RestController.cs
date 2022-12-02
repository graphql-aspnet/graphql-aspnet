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
        private static readonly RestResponsePayload _payload;

        static RestController()
        {
            _payload = new RestResponsePayload()
            {
                Data = new DonutPayload
                {
                    SingleDonut = new Donut()
                    {
                        Id = "5",
                        Name = "Static Rest Donut",
                        Flavor = "vanilla",
                    },
                },
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
            return this.Ok(_payload);
        }

        /// <summary>
        /// POST - /api/donuts/{id}.
        /// </summary>
        /// <param name="id">The donut id being updated.</param>
        /// <param name="donut">The inbound donut object.</param>
        /// <returns>Donut.</returns>
        [HttpPut("/api/donuts/{id}")]
        public IActionResult UpdateDonut(string id, [FromBody] Donut donut)
        {
            if (string.IsNullOrWhiteSpace(donut?.Id))
                return this.BadRequest();

            return this.Ok(_payload);
        }
    }
}