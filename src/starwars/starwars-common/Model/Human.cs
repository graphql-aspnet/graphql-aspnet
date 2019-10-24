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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A humanoid creature from the Star Wars universe.
    /// </summary>
    [GraphType]
    [Description("A humanoid creature from the Star Wars universe")]
    [DebuggerDisplay("Human: {Name}")]
    public class Human : ICharacter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Human"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Human(string id)
        {
            this.Id = (GraphId)id;
        }

        /// <summary>
        /// Gets the identifier of this human.
        /// </summary>
        /// <value>The identifier.</value>
        public GraphId Id { get;  }

        /// <summary>
        /// Gets or sets the name of this human.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the height of this human in meters.
        /// </summary>
        /// <value>The height in meters.</value>
        [GraphSkip]
        public float HeightInMeters { get; set; }

        /// <summary>
        /// Retrieves the human's hight, performing a conversion of units if necessary.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>System.Single.</returns>
        [GraphField]
        [Description("Height in the preferred unit, default is meters")]
        public float Height(LengthUnit unit = LengthUnit.Meter)
        {
            if (unit == LengthUnit.Meter)
                return this.HeightInMeters;
            return this.HeightInMeters * StarWarsConstants.FEET_IN_A_METER;
        }

        /// <summary>
        /// Gets or sets the mass of this human.
        /// </summary>
        /// <value>The mass.</value>
        [Description("Mass in kilograms, or null if unknown")]
        public float? Mass { get; set; }

        /// <summary>
        /// Gets or sets the home planet of this human.
        /// </summary>
        /// <value>The home planet.</value>
        [Description("The home planet of the human, or null if unknown")]
        public string HomePlanet { get; set; }

        /// <summary>
        /// Gets or sets a list of friends of this human.
        /// </summary>
        /// <value>The friends.</value>
        [GraphSkip]
        public IEnumerable<GraphId> FriendIds { get; set; }

        /// <summary>
        /// Gets or sets a collection of movie episodes the human appears in.
        /// </summary>
        /// <value>The appears in.</value>
        [Description("The movies this human appears in")]
        public IEnumerable<MovieEpisode> AppearsIn { get; set; }

        /// <summary>
        /// Gets or sets the starships this person has piloted.
        /// </summary>
        /// <value>The starships.</value>
        [GraphSkip]
        public IEnumerable<GraphId> StarshipIds { get; set; }
    }
}