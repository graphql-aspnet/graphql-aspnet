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
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// A humanoid creature from the Star Wars universe.
    /// </summary>
    [GraphType("Character")]
    [Description("A humanoid creature from the Star Wars universe")]
    public interface ICharacter
    {
        /// <summary>
        /// Gets the identifier of this character.
        /// </summary>
        /// <value>The identifier.</value>
        [Description("The ID of the character")]
        GraphId Id { get; }

        /// <summary>
        /// Gets the name of this character.
        /// </summary>
        /// <value>The name.</value>
        [Description("The name of the character")]
        string Name { get; }

        /// <summary>
        /// Gets a list of friends of this character.
        /// </summary>
        /// <value>The friends.</value>
        [GraphSkip]
        IEnumerable<GraphId> FriendIds { get; }

        /// <summary>
        /// Gets a collection of movie episodes the character appears in.
        /// </summary>
        /// <value>The appears in.</value>
        [Description("The movies this character appears in")]
        IEnumerable<MovieEpisode> AppearsIn { get; }
    }
}