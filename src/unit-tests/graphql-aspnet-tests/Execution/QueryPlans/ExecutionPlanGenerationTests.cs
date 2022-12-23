// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.PlanGenerationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionPlanGenerationTests
    {
        private IQueryDocument CreateDocument(GraphSchema schema, string text)
        {
            var docGenerator = new DefaultQueryDocumentGenerator<GraphSchema>(schema);
            var doc = docGenerator.CreateDocument(text.AsSpan());
            docGenerator.ValidateDocument(doc);
            return doc;
        }

        private async Task<IGraphQueryPlan> CreatePlan(GraphSchema schema, string text)
        {
            var doc = this.CreateDocument(schema, text);

            var planGenerator = new DefaultQueryPlanGenerator<GraphSchema>(
                schema,
                new DefaultQueryOperationDepthCalculator<GraphSchema>(),
                new DefaultQueryOperationComplexityCalculator<GraphSchema>());

            return await planGenerator.CreatePlanAsync(doc.Operations[0]);
        }

        [Test]
        public async Task SingleField_NoExtras_ValidateFields()
        {
            var server = new TestServerBuilder().AddType<SimplePlanGenerationController>().Build();
            var plan = await this.CreatePlan(
                server.Schema,
                "query {  simple {  simpleQueryMethod { property1} } }");

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            // the "simple" virtual field queued to be resolved when the plan
            // is executed
            var queuedContext = plan.Operation.FieldContexts[0];
            Assert.IsNotNull(queuedContext);
            Assert.AreEqual("simple", queuedContext.Name);
            Assert.AreEqual("simple", queuedContext.Origin.Path.ToDotString());

            // "simple" should contain 1 child field called "simpleQueryMethod"
            Assert.AreEqual(0, queuedContext.Arguments.Count);
            Assert.AreEqual(1, queuedContext.ChildContexts.Count);
            Assert.IsTrue(queuedContext.Field?.Resolver is GraphControllerRouteFieldResolver);

            // simpleQueryMethod should contain 1 property to be resolved
            var child = queuedContext.ChildContexts[0];
            Assert.IsNotNull(child);
            Assert.AreEqual("simpleQueryMethod", child.Name);
            Assert.AreEqual("simple.simpleQueryMethod", child.Origin.Path.ToDotString());
            Assert.AreEqual(1, child.ChildContexts.Count);

            // the defaults defined on the method should have been assigned when none were supplied
            // since both were optional.
            Assert.AreEqual(2, child.Arguments.Count);
            Assert.IsTrue(child.Field?.Resolver is GraphControllerActionResolver);

            var arg1 = child.Arguments["arg1"];
            var arg2 = child.Arguments["arg2"];
            Assert.AreEqual("default string", arg1.Value.Resolve(ResolvedVariableCollection.Empty));
            Assert.AreEqual(5, arg2.Value.Resolve(ResolvedVariableCollection.Empty));

            // "property1"
            var prop1 = child.ChildContexts[0];
            Assert.IsNotNull(prop1);
            Assert.AreEqual("property1", prop1.Name);
            Assert.AreEqual("simple.simpleQueryMethod.property1", prop1.Origin.Path.ToDotString());
            Assert.AreEqual(0, prop1.ChildContexts.Count);
            Assert.AreEqual(0, prop1.Arguments.Count);
            Assert.IsTrue(prop1.Field?.Resolver is ObjectPropertyResolver);
        }

        [Test]
        public async Task SingleField_WithAcceptableArgumentsOnMethod_ValidateFields()
        {
            var server = new TestServerBuilder()
                .AddType<SimplePlanGenerationController>()
                .Build();

            var plan = await this.CreatePlan(
              server.Schema,
              "query {  simple {  simpleQueryMethod(arg1: \"bob\", arg2: 15) { property1} } }");

            Assert.IsNotNull(plan.Operation);
            Assert.AreEqual(0, plan.Messages.Count);
            var queuedContext = plan.Operation.FieldContexts[0];
            var child = queuedContext.ChildContexts[0];

            var arg1 = child.Arguments["arg1"];
            var arg2 = child.Arguments["arg2"];
            Assert.IsNotNull(arg1);
            Assert.IsNotNull(arg2);
            Assert.AreEqual("bob", arg1.Value.Resolve(ResolvedVariableCollection.Empty));
            Assert.AreEqual(15L, arg2.Value.Resolve(ResolvedVariableCollection.Empty));
        }

        [Test]
        public async Task SingleField_WithVariables_UsingDefaults_WithAcceptableArgumentsOnMethod_VariableValueIsAssigned()
        {
            var server = new TestServerBuilder().AddType<SimplePlanGenerationController>().Build();

            var plan = await this.CreatePlan(
                server.Schema,
                @"query($var1 : Long = 22)
                {
                    simple {
                        simpleQueryMethod(arg1: ""bob"", arg2: $var1) {
                            property1
                        }
                    }
                }");

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            var queuedContext = plan.Operation.FieldContexts[0];
            var child = queuedContext.ChildContexts[0];

            // Ensur the variable $var1 used its default value and that arg2 is assigned that value
            var arg2 = child.Arguments["arg2"];
            Assert.IsNotNull(arg2);
        }

        [Test]
        public async Task SingleField_WhenInputArgumentPassesNull_WhenAcceptable_GeneratesArgumentAsNull()
        {
            var server = new TestServerBuilder().AddType<SimplePlanGenerationController>().Build();
            var plan = await this.CreatePlan(
                server.Schema,
                @"query {
                    simple {
                        complexQueryMethod(arg1: null) {
                            property1
                        }
                    }
                }");

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            var queuedContext = plan.Operation.FieldContexts[0];
            var child = queuedContext.ChildContexts[0];

            // Ensure that arg1 exists and is recieved as null
            var arg1 = child.Arguments["arg1"];

            Assert.IsNull(arg1.Value.Resolve(ResolvedVariableCollection.Empty));
        }

        [Test]
        public async Task SingleField_WithVariables_UsingDefaultValues_NestedInInputObjects_YieldsCorrectInputObject()
        {
            var server = new TestServerBuilder().AddType<SimplePlanGenerationController>().Build();

            // arg1 represents a TWoPropertyObjectV2 with a prop1 type of float
            var plan = await this.CreatePlan(
                server.Schema,
                @"query($var1 : Float! = 15.5)
                {
                    simple {
                        complexQueryMethod(arg1: { property1: $var1, property2: 0} ) {
                            property1
                        }
                    }
                }");

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            var queuedContext = plan.Operation.FieldContexts[0];

            Assert.AreEqual(1, queuedContext.ChildContexts.Count);
            var child = queuedContext.ChildContexts[0];

            // Ensure the variable $var1 is used as the value of property1
            // using hte default value in the query
            var arg1 = child.Arguments["arg1"];
            Assert.IsNotNull(arg1);
        }

        [Test]
        public async Task SingleField_WithFragment_ValidateFields()
        {
            var server = new TestServerBuilder().AddType<SimplePlanGenerationController>().Build();

            var query = @"
                        query {
                            simple {
                                simpleQueryMethod {
                                    ...methodProperties
                                }
                            }
                        }

                        fragment methodProperties on TwoPropertyObject {
                            property1
                            property2
                        }";

            var plan = await this.CreatePlan(server.Schema, query);

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            // the "simple" virtual field queued to be resolved when the plan
            // is executed
            var queuedContext = plan.Operation.FieldContexts[0];
            Assert.IsNotNull(queuedContext);
            Assert.AreEqual("simple", queuedContext.Name);

            // "simple" should contain 1 child field called "simpleQueryMethod"
            Assert.AreEqual(0, queuedContext.Arguments.Count);
            Assert.AreEqual(1, queuedContext.ChildContexts.Count);
            Assert.IsTrue(queuedContext.Field?.Resolver is GraphControllerRouteFieldResolver);

            // simpleQueryMethod should contain 2 properties to be resolved (the two props on the fragment)
            var child = queuedContext.ChildContexts[0];
            Assert.IsNotNull(child);
            Assert.AreEqual("simpleQueryMethod", child.Name);
            Assert.AreEqual(2, child.ChildContexts.Count);

            // the defaults defined on the method should have been assigned when none were supplied
            // but both were optional.
            Assert.AreEqual(2, child.Arguments.Count);
            Assert.IsTrue(child.Field?.Resolver is GraphControllerActionResolver);

            // "property1"
            var prop1 = child.ChildContexts[0];
            Assert.IsNotNull(prop1);
            Assert.AreEqual("property1", prop1.Name);
            Assert.AreEqual(0, prop1.ChildContexts.Count);
            Assert.AreEqual(0, prop1.Arguments.Count);
            Assert.IsTrue(prop1.Field?.Resolver is ObjectPropertyResolver);

            // "property2"
            var prop2 = child.ChildContexts[1];
            Assert.IsNotNull(prop2);
            Assert.AreEqual("property2", prop2.Name);
            Assert.AreEqual(0, prop2.ChildContexts.Count);
            Assert.AreEqual(0, prop2.Arguments.Count);
            Assert.IsTrue(prop2.Field?.Resolver is ObjectPropertyResolver);
        }

        [Test]
        public async Task MultipleTypeRestrictedFragments_GeneratesCorrectFieldContexts()
        {
            var server = new TestServerBuilder().AddType<FragmentProcessingController>().Build();

            var query = @"
                        query {
                            fragTester {
                                makeHybridData {
                                    ...aData
                                    ...bData
                                }
                            }
                        }

                        fragment aData on FragmentDataA{
                            property1
                        }

                        fragment bData on FragmentDataB{
                            property2
                        }";

            var plan = await this.CreatePlan(server.Schema, query);

            Assert.IsNotNull(plan?.Operation);
            Assert.AreEqual(0, plan.Messages.Count);

            var fragTester = plan.Operation.FieldContexts[0];
            Assert.IsNotNull(fragTester);

            var makeHybridData = fragTester.ChildContexts[0];
            Assert.IsNotNull(makeHybridData);
            Assert.AreEqual(2, makeHybridData.ChildContexts.Count);
            Assert.IsTrue(makeHybridData.ChildContexts.Any<IGraphFieldInvocationContext>(x => x.ExpectedSourceType == typeof(FragmentDataA) && x.Field.Name == "property1"));
            Assert.IsTrue(makeHybridData.ChildContexts.Any(x => x.ExpectedSourceType == typeof(FragmentDataB) && x.Field.Name == "property2"));
        }

        [Test]
        public async Task MultipleTypeRestrictedFragments_GeneratesCorrectFieldContexts_WithDuplciatedFieldNamesOnFragments()
        {
            var server = new TestServerBuilder()
                .AddType<FragmentProcessingController>()
                .Build();

            var query = @"
                        query {
                            fragTester {
                                makeHybridData {
                                    ...aData
                                    ...bData
                                }
                            }
                        }

                        fragment aData on FragmentDataA{
                            property1
                        }

                        fragment bData on FragmentDataB{
                            property1
                        }";

            var plan = await this.CreatePlan(server.Schema, query);

            Assert.IsNotNull(plan);
            Assert.IsNotNull(plan.Operation);
            Assert.AreEqual(0, plan.Messages.Count);

            var fragTester = plan.Operation.FieldContexts[0];
            Assert.IsNotNull(fragTester);

            var makeHybridData = fragTester.ChildContexts[0];
            Assert.IsNotNull(makeHybridData);
            Assert.AreEqual(2, makeHybridData.ChildContexts.Count);
            Assert.IsTrue(makeHybridData.ChildContexts.Any(x => x.ExpectedSourceType == typeof(FragmentDataA) && x.Field.Name == "property1"));
            Assert.IsTrue(makeHybridData.ChildContexts.Any(x => x.ExpectedSourceType == typeof(FragmentDataB) && x.Field.Name == "property1"));
        }

        [Test]
        public async Task WhenTheSameFieldIsReferencedMoreThanOnce_ForAGivenType_FieldsAreMerged_Correctly()
        {
            var server = new TestServerBuilder().AddType<SimplePlanGenerationController>().Build();

            // Property 1 is referenced in the query and in the fragment such that when spread prop1 would be included twice
            // at the same level
            //
            // These two instnaces can be merged and Property 1 (for the TwoPropertyObject) should only end up in the plan once
            var query = @"
                        query {
                            simple {
                                simpleQueryMethod {
                                    property1
                                    ...methodProperties
                                }
                            }
                        }

                        fragment methodProperties on TwoPropertyObject {
                            property1
                            property2
                        }";

            var plan = await this.CreatePlan(server.Schema, query);

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            // simple -> SimpleQueryMthod
            var simpleQueryMethodField = plan.Operation.FieldContexts[0].ChildContexts[0];

            Assert.AreEqual(2, simpleQueryMethodField.ChildContexts.Count);
            Assert.IsTrue(simpleQueryMethodField.ChildContexts.Any(x => x.Name == "property1"));
            Assert.IsTrue(simpleQueryMethodField.ChildContexts.Any(x => x.Name == "property2"));
        }

        [Test]
        public async Task Union_TypeName_AsAFieldSelectionDirectlyOnAUnion_ProducesTypeNameFieldForAllMembersOfTheUnion()
        {
            var server = new TestServerBuilder()
                .AddType<SimplePlanGenerationController>()
                .Build();

            // unionQuery returns a union graphtype of TwoPropObject and TwoPropObjectV2
            // specific fields for V1 are requested but V2 should be included with __typename as well
            var query = @"
                        query {
                            simple {
                                unionQuery {
                                    ...RequestedProperties
                                    __typename
                                }
                            }
                        }

                        fragment RequestedProperties on TwoPropertyObject {
                            property1
                            property2
                        }";

            var plan = await this.CreatePlan(server.Schema, query);

            Assert.IsNotNull(plan);
            Assert.AreEqual(0, plan.Messages.Count);
            Assert.IsNotNull(plan.Operation);

            // simple -> SimpleQueryMthod
            var unionQueryField = plan.Operation.FieldContexts[0].ChildContexts[0];

            // prop1 for TwoPropObject, prop2 fro TwoPropObject, __typename for TwoPropObject, __typename for TwoPropObjectV2
            Assert.AreEqual(4, unionQueryField.ChildContexts.Count);
            Assert.IsTrue(unionQueryField.ChildContexts.Any(x => x.Name == "property1" && x.ExpectedSourceType == typeof(TwoPropertyObject)));
            Assert.IsTrue(unionQueryField.ChildContexts.Any(x => x.Name == "property2" && x.ExpectedSourceType == typeof(TwoPropertyObject)));
            Assert.IsTrue(unionQueryField.ChildContexts.Any(x => x.Name == "__typename" && x.ExpectedSourceType == typeof(TwoPropertyObject)));
            Assert.IsTrue(unionQueryField.ChildContexts.Any(x => x.Name == "__typename" && x.ExpectedSourceType == typeof(TwoPropertyObjectV2)));
        }

        [Test]
        public async Task Union_WhenAnInterfaceIsSpreadInAUnion_ShouldReturnTheInterfaceFields_ForEachMemberOfTheUnionThatImplementsTheInterface()
        {
            var server = new TestServerBuilder()
                .AddType<SimplePlanGenerationController>()
                .AddType<ITwoPropertyObject>()
                .Build();

            // unionQuery returns a union graphtype of (TwoPropObject | TwoPropObjectV2)
            // TwoPropObject implement the interface graphtype TwoPropertyInterface and property1 should
            // be included in hte plan for that object graph type reference
            // since TwoPropObjectV2 does not implement TwoPropertyInterface it should not be included
            var query = @"
                        query {
                            simple {
                                unionQuery {
                                    ...RequestedProperties
                                }
                            }
                        }

                        fragment RequestedProperties on TwoPropertyInterface {
                            property1
                        }";

            var plan = await this.CreatePlan(server.Schema, query);

            Assert.IsNotNull(plan?.Operation);
            Assert.AreEqual(0, plan.Messages.Count);

            // simple -> SimpleQueryMthod
            var unionQueryField = plan.Operation.FieldContexts[0].ChildContexts[0];

            // prop1 for TwoPropObject
            Assert.AreEqual(1, unionQueryField.ChildContexts.Count);
            Assert.IsTrue(unionQueryField.ChildContexts.Any(x => x.Name == "property1" && x.ExpectedSourceType == typeof(TwoPropertyObject)));
        }
    }
}