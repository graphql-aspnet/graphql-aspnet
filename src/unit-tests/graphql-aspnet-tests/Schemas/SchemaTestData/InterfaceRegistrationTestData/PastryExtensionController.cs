// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData.InterfaceRegistrationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class PastryExtensionController : GraphController
    {
        [TypeExtension(typeof(IPastry), "hasSugar", typeof(bool))]
        public bool HasSugarExtension(IPastry pastry)
        {
            return pastry is IDonut;
        }

        [TypeExtension(typeof(IDonut), "hasGlaze", typeof(bool))]
        public bool HasGlazeExtension(IDonut pastry)
        {
            return pastry.Name.Contains("glazed");
        }

        [TypeExtension(typeof(Donut), "hasDoubleGlaze", typeof(bool))]
        public bool HasDoubleGlazeExtension(Donut pastry)
        {
            return pastry.Name.Contains("doubleglazed");
        }
    }
}