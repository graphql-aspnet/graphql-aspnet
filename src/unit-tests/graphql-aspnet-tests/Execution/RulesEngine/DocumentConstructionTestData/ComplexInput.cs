// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.RulesEngine.DocumentConstructionTestData
{
    using System.Collections.Generic;

    public class ComplexInput
    {
        public List<Donut> Donuts { get; set; }

        public List<Bagel> Bagels { get; set; }

        public Donut SingleDonut { get; set; }

        public Bagel SingleBagel { get; set; }
    }
}