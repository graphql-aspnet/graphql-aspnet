// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting
{
    /// <summary>
    /// A set of internally supported name formatting options for various names and values created
    /// for a schema.
    /// </summary>
    public enum GraphNameFormatStrategy
    {
        ProperCase,
        CamelCase,
        UpperCase,
        LowerCase,
        NoChanges,
    }
}