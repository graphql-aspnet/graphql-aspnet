﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.RulesEngine.RuleCheckTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType("PeopleMover")]
    public interface IPeopleMover
    {
        int Id { get; }

        string Name { get; }
    }
}