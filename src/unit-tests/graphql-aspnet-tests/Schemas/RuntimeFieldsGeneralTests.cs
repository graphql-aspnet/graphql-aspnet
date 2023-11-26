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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeFieldsGeneralTests
    {
        [Test]
        public void InternalName_OnQueryField_IssCarriedToSchema()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("field1", () => 1)
                        .WithInternalName("field1_internal_name");
                })
                .Build();

            var operation = server.Schema.Operations[GraphOperationType.Query];
            var field = operation.Fields.FindField("field1");
            Assert.AreEqual("field1_internal_name", field.InternalName);
        }

        [Test]
        public void InternalName_OnMutationField_IssCarriedToSchema()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapMutation("field1", () => 1)
                        .WithInternalName("field1_internal_name");
                })
                .Build();

            var operation = server.Schema.Operations[GraphOperationType.Mutation];
            var field = operation.Fields.FindField("field1");
            Assert.AreEqual("field1_internal_name", field.InternalName);
        }

        [Test]
        public void InternalName_OnTypeExension_IssCarriedToSchema()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<TwoPropertyObject>();
                    o.MapTypeExtension<TwoPropertyObject>("field1", () => 1)
                    .WithInternalName("extension_field_Internal_Name");
                })
                .Build();

            var obj = server.Schema.KnownTypes.FindGraphType(typeof(TwoPropertyObject)) as IObjectGraphType;
            var field = obj.Fields.FindField("field1");
            Assert.AreEqual("extension_field_Internal_Name", field.InternalName);
        }

        [Test]
        public void InternalName_OnDirective_IsCarriedToSchema()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapDirective("@myDirective", () => GraphActionResult.Ok())
                    .WithInternalName("directive_internal_name");
                })
                .Build();

            var dir = server.Schema.KnownTypes.FindDirective("myDirective");
            Assert.AreEqual("directive_internal_name", dir.InternalName);
        }
    }
}