// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Schemas
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Schemas.SchemaTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphSchemaManagerTests
    {
        [Test]
        public void AddSingleSubscriptionAction_AllDefaults_EnsureFieldStructure()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = new SubscriptionEnabledTemplateProvider();
            var schema = new GraphSchema() as ISchema;
            schema.SetNoAlterationConfiguration();
            schema.SetSubscriptionAllowances();

            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType<SimpleMethodController>();

            // query always exists
            // subscription root was found via the method parsed
            // mutation was not provided
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Query));
            Assert.IsFalse(schema.OperationTypes.ContainsKey(GraphCollection.Mutation));
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Subscription));

            // field for the controller exists
            var topFieldName = nameof(SimpleMethodController).Replace(Constants.CommonSuffix.CONTROLLER_SUFFIX, string.Empty);
            Assert.IsTrue(schema.OperationTypes[GraphCollection.Subscription].Fields.ContainsKey(topFieldName));

            // ensure the field on the subscription operation  is the right name (i.e. the controller name)
            var topField = schema.OperationTypes[GraphCollection.Subscription][topFieldName];
            Assert.IsNotNull(topField);

            var type = schema.KnownTypes.FindGraphType(topField) as IObjectGraphType;

            var action = TemplateHelper.CreateFieldTemplate<SimpleMethodController>(nameof(SimpleMethodController.TestActionMethod));

            // ensure the action was put into the field collection of the controller operation
            Assert.IsTrue(type.Fields.ContainsKey(action.Route.Name));
        }

        [Test]
        public void AddASubscriptionAction_WithoutUpdatingTheConfiguration_ThrowsDeclarationException()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();

            GraphQLProviders.TemplateProvider = new SubscriptionEnabledTemplateProvider();
            var schema = new GraphSchema() as ISchema;
            schema.SetNoAlterationConfiguration();

            // do not tell the schema to allow the subscription operation type
            // schema.SetSubscriptionAllowances();

            // attempt to add a controller with a subscription
            Assert.Throws<ArgumentOutOfRangeException>(() =>
           {
               var manager = new GraphSchemaManager(schema);
               manager.EnsureGraphType<SimpleMethodController>();
           });
        }
    }
}