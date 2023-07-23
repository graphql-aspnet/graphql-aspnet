// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System.Globalization;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
    using NUnit.Framework;

    [TestFixture]
    public class GeneralSchemaTests
    {
        [Test]
        public void InternalName_OnControllerAction_IsRendered()
        {
            var schema = new TestServerBuilder()
                .AddController<ControllerWithInternalNames>()
                .Build().Schema;

            var field = schema.Operations[GraphOperationType.Query].Fields.FindField("actionField");
            Assert.AreEqual("ActionWithInternalName", field.InternalName);
        }

        [Test]
        public void InternalName_OnTypeExension_IsRendered()
        {
            var schema = new TestServerBuilder()
                .AddController<ControllerWithInternalNames>()
                .Build().Schema;

            var twoProp = schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject)) as IObjectGraphType;
            var field = twoProp.Fields.FindField("field1");
            Assert.AreEqual("TypeExtensionInternalName", field.InternalName);
        }
    }
}