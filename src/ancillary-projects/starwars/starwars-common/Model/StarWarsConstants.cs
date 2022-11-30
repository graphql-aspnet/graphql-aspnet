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
    using System;

    /// <summary>
    /// A set of known constants needed in the star wars api.
    /// </summary>
    public static class StarWarsConstants
    {
        /// <summary>
        /// The number of feet in a meter.
        /// </summary>
        public const float FEET_IN_A_METER = 3.28084f;

        /// <summary>
        /// Never reference jar jar binks. . . its better that way.
        /// </summary>
        [Obsolete]
        public static readonly GraphId JarJarId = new GraphId("-error-");

        /// <summary>
        /// Luke's id in this system held as a referenced constant for easy access.
        /// </summary>
        public static readonly GraphId LukeSkywalkerId = new GraphId("1000");

        /// <summary>
        /// R2-D2's id in the system, held as a referenced constant for easy access.
        /// </summary>
        public static readonly GraphId R2D2Id = new GraphId("2001");

        /// <summary>
        /// Initializes static members of the <see cref="StarWarsConstants"/> class.
        /// </summary>
        static StarWarsConstants()
        {
        }
    }
}