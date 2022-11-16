﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.InheritanceTestData
{
    public class Box2 : IBox
    {
        public string Height { get; set; }

        public string Length { get; set; }

        public string Width { get; set; }
    }
}