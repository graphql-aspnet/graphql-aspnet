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
    /// An enumeration describing how lengths can be outputted in the object graph.
    /// </summary>
    [GraphType]
    [Description("Units of height")]
    public enum LengthUnit
    {
        [Description("The standard unit around the world")]
        Meter,

        [Description("Primarily used in the United States")]
        Foot,
    }
}