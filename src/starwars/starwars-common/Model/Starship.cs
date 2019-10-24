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
    /// A startship in the star wars universe.
    /// </summary>
    [GraphType]
    [DebuggerDisplay("Starship: {Name}")]
    [Description("A starship from the Star Wars universe")]
    public class Starship
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Starship"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Starship(string id)
        {
            this.Id = new GraphId(id);
        }

        /// <summary>
        /// Gets the identifier of the starship.
        /// </summary>
        /// <value>The identifier.</value>
        [Description("The ID of the starship")]
        public GraphId Id { get; }

        /// <summary>
        /// Gets or sets the name of the starship.
        /// </summary>
        /// <value>The name.</value>
        [Description("The name of the starship")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the length of the startship in meters.
        /// </summary>
        /// <value>The length in meters.</value>
        [GraphSkip]
        public float LengthInMeters { get; set; }

        /// <summary>
        /// Retrieves the length of this starship in the given units provided.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>System.Single.</returns>
        [GraphField]
        [Description("Length of the starship, along the longest axis")]
        public float Length(LengthUnit unit = LengthUnit.Meter)
        {
            if (unit == LengthUnit.Meter)
                return this.LengthInMeters;
            return this.LengthInMeters * StarWarsConstants.FEET_IN_A_METER;
        }

        /// <summary>
        /// Gets or sets the coordinates of the starship.
        /// </summary>
        /// <value>The coordinates.</value>
        [GraphField]
        public IEnumerable<IEnumerable<float>> Coordinates { get; set; }
    }
}