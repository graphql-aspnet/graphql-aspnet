﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.IntrospecetionInputFieldTestData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class NotRequiredNotSetGuidObject
    {
        public Guid Property1 { get; set; }
    }
}