// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Execution.TestData.UnionTypeExecutionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class UnionTypeExecutionTests
    {
        [Test]
        public async Task WhenTeacherReturned_TeacherFieldsResolved()
        {
            var server = new TestServerBuilder()
                .AddType<Person>()
                .AddType<Teacher>()
                .AddGraphController<SchoolController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrievePerson(id: 1) {
                            ... on Teacher {
                                id
                                name
                                numberOfStudents
                            }
                            ... on Person {
                                id
                                name
                            }
                        }
                    }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrievePerson"" : {
                            ""id"" : 1,
                            ""name"": ""Teacher1"",
                            ""numberOfStudents"" : 15
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task WhenUnknownTypeReturned_PersonFieldsResolved()
        {
            var server = new TestServerBuilder()
                .AddType<Person>()
                .AddType<Teacher>()
                .AddGraphController<SchoolController>()
                .Build();

            // with id 2 the controller method returns
            // a Student object but student is unknown to the schema
            //
            // The union mapper should resolve it to a Person type
            // the only type in the union to which it can be applied.
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrievePerson(id: 2) {
                            ... on Teacher {
                                id
                                name
                                numberOfStudents
                            }
                            ... on Person {
                                id
                                name
                            }
                        }
                    }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrievePerson"" : {
                            ""id"" : 2,
                            ""name"": ""Student1"",
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task WhenPersonTypeReturned_PersonFieldsResolved()
        {
            var server = new TestServerBuilder()
                .AddType<Person>()
                .AddType<Teacher>()
                .AddGraphController<SchoolController>()
                .Build();

            // with id 3 the controller method returns
            // a HeaderTeach object which could match to Teacher and a Person
            //
            // this type is also unknown to the schema so the schema
            // does not have enough information to determine what type
            // Headteacher should masqurade as (Teacher or Person)
            //
            // The typemapper of the union should be invoked to
            // determine the correct type, in the case of this union
            // anything not explicitly a teacher is simply a person.
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrievePerson(id: 3) {
                            ... on Teacher {
                                id
                                name
                                numberOfStudents
                            }
                            ... on Person {
                                id
                                name
                            }
                        }
                    }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrievePerson"" : {
                            ""id"" : 3,
                            ""name"": ""HeadMaster"",
                        }
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task WhenUnmatchedTypeReturnsUnHandlableType_QueryFails()
        {
            var server = new TestServerBuilder()
                .AddType<Person>()
                .AddType<Teacher>()
                .AddGraphController<SchoolController>()
                .Build();

            // with id 5 the controller method returns
            // a TwoPropertyObject which matches none of the allowed
            // types for the union. The union is instructed to return typeof(TwoPropertyObject)
            // when its is received
            //
            // this should cause a failure of the query because TwoPropertyObject
            // cant be handled by the union
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrievePerson(id: 5) {
                            ... on Teacher {
                                id
                                name
                                numberOfStudents
                            }
                            ... on Person {
                                id
                                name
                            }
                        }
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsFalse(result.Messages.IsSucessful);

            var rule = result.Messages[0].MetaData["Rule"];
            Assert.AreEqual("6.4.3", rule);
        }

        [Test]
        public async Task WhenUnmatchedTypeReturnsNull_QueryFails()
        {
            var server = new TestServerBuilder()
                .AddType<Person>()
                .AddType<Teacher>()
                .AddGraphController<SchoolController>()
                .Build();

            // with id 6 the controller method returns
            // a TwoPropertyObjectV3 which matches none of the allowed
            // types for the union. The union is instructed to return null
            // when this type is attempted to be mapped
            //
            // this should cause the query to fail because of the null return
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrievePerson(id: 6) {
                            ... on Teacher {
                                id
                                name
                                numberOfStudents
                            }
                            ... on Person {
                                id
                                name
                            }
                        }
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.IsFalse(result.Messages.IsSucessful);

            var rule = result.Messages[0].MetaData["Rule"];
            Assert.AreEqual("6.4.3", rule);
        }

        [Test]
        public async Task WhenTypeMapperThrowsException_ExceptionIsAllowedToBubble()
        {
            var server = new TestServerBuilder()
                .AddType<Person>()
                .AddType<Teacher>()
                .AddGraphController<SchoolController>()
                .Build();

            // with id 7 the controller method returns
            // a TwoPropertyObjectV2 which matches none of the allowed
            // types for the union. The union is instructed to throw
            // an exception for this scenario.
            //
            // However, the type system should see that V2 could NEVER
            // map to one of the allowed types and should fail on rule 6.4.3
            // without ever calling into the type mapper

            //
            // this should cause the query to fail
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrievePerson(id: 7) {
                            ... on Teacher {
                                id
                                name
                                numberOfStudents
                            }
                            ... on Person {
                                id
                                name
                            }
                        }
                    }");

            try
            {
                var result = await server.ExecuteQuery(builder);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Union Threw an Exception", ex.Message);
                return;
            }

            Assert.Fail("Expected a specific unhandled exception from the failed union, got none");
        }

        [Test]
        public async Task WhenMultipleUnrelatedTypesAreReturnedGenerally_AndOneExecutesABatchExtensionForAnImplementedInterface_QueryExecutesCorrectly()
        {
            var server = new TestServerBuilder(TestOptions.IncludeExceptions)
                .AddType<Television>()
                .AddType<Person>()
                .AddType<Home>()
                .AddType<IBuilding>()
                .AddGraphController<UnrelatedItemsController>()
                .Build();

            // The Items on retrieveItems are not related (Person, Home, TV)
            // The method returns the unioned items as a List<object> to the runtime
            //
            // IBuilding, which home implements, has `.perimeter` built as a batch extension
            // and needs to take inthe IEnumerable<IBuilding>
            //
            // The runtime must properlty detect and cast the right items of List<object>
            // to a single IEnumerable<IBuilding> to correctly invoke the batch extension
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrieveItems {
                            ... on Home {
                                id
                                name

                                #this is a batch extension
                                perimeter
                            }
                        }
                    }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveItems"" : [{
                            ""id"" : 1,
                            ""name"": ""Home 1"",
                            ""perimeter"": 1000
                        },
                        {
                            ""id"" : 2,
                            ""name"": ""Home 2"",
                            ""perimeter"": 1400
                        }]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task WhenMultipleUnrelatedTypesAreReturnedGenerally_AndOneExecutesATypeExtensionForAnImplementedInterface_QueryExecutesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddType<Television>()
                .AddType<Person>()
                .AddType<Home>()
                .AddType<IBuilding>()
                .AddGraphController<UnrelatedItemsController>()
                .Build();

            // The items on retrieveItems are not related (Person, Home, TV)
            // The method returns the unioned items as a List<object> to the runtime
            //
            // IBuilding, which home implements, has `.squareFeet` built as a type extension
            // and needs to take in an IBuilding
            //
            // The runtime must detect and properly cast the right objects in the unioned list
            // to IBuilding to correctly invoke the type extension
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        retrieveItems {
                            ... on Home {
                                id
                                name

                                #this is a single item type extension
                                squareFeet
                            }
                        }
                    }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveItems"" : [{
                            ""id"" : 1,
                            ""name"": ""Home 1"",
                            ""squareFeet"": 60000
                        },
                        {
                            ""id"" : 2,
                            ""name"": ""Home 2"",
                            ""squareFeet"": 120000
                        }]
                     }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}