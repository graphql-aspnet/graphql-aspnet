// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.QueryLanguageTestData
{
    using GraphQL.AspNet.Attributes;

    public enum Happiness
    {
        Happy,
        Sad,

        [GraphSkip]
        Melancholy,
    }
}