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
    using System.ComponentModel.DataAnnotations;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.SubscriberLoadTest.Models.Models.Server;

    /// <summary>
    /// A controller for working with donuts.
    /// </summary>
    public class DonutController : GraphController
    {
        private static readonly Donut _donut;

        static DonutController()
        {
            _donut = new Donut()
            {
                Id = "1",
                Name = "Static GraphQL Donut",
                Flavor = DonutFlavor.Chocolate,
            };
        }

        /// <summary>
        /// Retrieves a single donut.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>List&lt;Donut&gt;.</returns>
        [QueryRoot(typeof(Donut))]
        public IGraphActionResult RetrieveDonut(string id)
        {
            return this.Ok(_donut);
        }

        /// <summary>
        /// If the donut id exists, updates it with the given values, otherwise adds it to the list.
        /// </summary>
        /// <param name="donut">The donut to modify.</param>
        /// <returns>IGraphActionResult.</returns>
        [MutationRoot(typeof(Donut))]
        public IGraphActionResult AddOrUpdateDonut(Donut donut)
        {
            this.PublishSubscriptionEvent("DONUT_UPDATED", _donut);
            return this.Ok(_donut);
        }

        /// <summary>
        /// A subscription called when a donut is updated or added to the system.
        /// </summary>
        /// <param name="donut">The donut.</param>
        /// <param name="id">The identifier of the donut to watch for.</param>
        /// <returns>IGraphActionResult.</returns>
        [SubscriptionRoot("onDonutUpdated", typeof(Donut), EventName = "DONUT_UPDATED")]
        public IGraphActionResult OnDonutUpdated(Donut donut, [Required] string id)
        {
            if (donut.Id == id)
                return this.Ok(donut);

            return this.SkipSubscriptionEvent();
        }
    }
}