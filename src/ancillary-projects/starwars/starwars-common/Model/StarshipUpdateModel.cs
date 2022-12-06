// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.StarwarsAPI.Common.Model
{
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A model object used for updating the name of a starship via a mutation.
    /// </summary>
    [GraphType(InputName = "StarshipData")]
    public class StarshipUpdateModel
    {
        /// <summary>
        /// Gets or sets the identifier of the starship to update.
        /// </summary>
        /// <value>The identifier.</value>
        public GraphId Id { get; set; }

        /// <summary>
        /// Gets or sets the new name to assign to the starship.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}