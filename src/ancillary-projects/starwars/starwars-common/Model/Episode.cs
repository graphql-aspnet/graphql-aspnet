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
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;

    /// <summary>
    /// An enumeration possible episodes supported by this api.
    /// </summary>
    [GraphType("Episode")]
    [Description("The episodes in the Star Wars trilogy")]
    public enum MovieEpisode
    {
        [Description("Star Wars Episode IV: A New Hope, released in 1977.")]
        NewHope,

        [Description("Star Wars Episode V: The Empire Strikes Back, released in 1980.")]
        Empire,

        [Description("Star Wars Episode VI: Return of the Jedi, released in 1983.")]
        Jedi,
    }
}