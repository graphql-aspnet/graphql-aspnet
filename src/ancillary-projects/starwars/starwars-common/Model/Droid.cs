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
    /// An autonomous mechanical character in the Star Wars universe.
    /// </summary>
    [GraphType]
    [Description("An autonomous mechanical character in the Star Wars universe")]
    [DebuggerDisplay("Droid: {Name}")]
    public class Droid : ICharacter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Droid"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Droid(string id)
        {
            this.Id = (GraphId)id;
        }

        /// <summary>
        /// Gets the identifier of this droid.
        /// </summary>
        /// <value>The identifier.</value>
        [Description("The ID of the droid")]
        public GraphId Id { get; }

        /// <summary>
        /// Gets or sets the name of this droid.
        /// </summary>
        /// <value>The name.</value>
        [Description("What others call this droid")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a list of friends of this droid.
        /// </summary>
        /// <value>The friends.</value>
        [GraphSkip]
        public IEnumerable<GraphId> FriendIds { get; set; }

        /// <summary>
        /// Gets or sets a collection of movie episodes the droid appears in.
        /// </summary>
        /// <value>The appears in.</value>
        [Description("The movies this droid appears in")]
        public IEnumerable<MovieEpisode> AppearsIn { get; set; }

        /// <summary>
        /// Gets or sets the primary function of this droid.
        /// </summary>
        /// <value>The primary function.</value>
        [Description("This droid's primary function")]
        public string PrimaryFunction { get; set; }
    }
}