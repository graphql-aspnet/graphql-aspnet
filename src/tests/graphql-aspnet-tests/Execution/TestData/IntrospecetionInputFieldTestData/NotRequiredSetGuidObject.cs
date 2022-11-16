// *************************************************************
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

    public class NotRequiredSetGuidObject
    {
        public NotRequiredSetGuidObject()
        {
            this.Property1 = Guid.Parse("033979ae-0955-4ef6-8a37-50bf0359601f");
        }

        public Guid Property1 { get; set; }
    }
}