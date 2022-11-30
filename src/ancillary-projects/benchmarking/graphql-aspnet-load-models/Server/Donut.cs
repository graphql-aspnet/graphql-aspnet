// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.Models.Models.Server
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A server side representation of a donut.
    /// </summary>
    public class Donut
    {
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Donut.</returns>
        public Donut Clone()
        {
            return new Donut()
            {
                Id = Id,
                Name = Name,
                Flavor = Flavor,
            };
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the flavor.
        /// </summary>
        /// <value>The flavor.</value>
        public DonutFlavor Flavor { get; set; }
    }
}