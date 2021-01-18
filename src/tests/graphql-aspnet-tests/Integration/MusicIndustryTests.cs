// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Integration
{
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Integration.Model;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    /// <summary>
    /// <para>
    /// The tests in this class represent a "real world" scenario with a semi-realistic object model attempting to use the library
    /// in a manner that a developer would with model classes, type extensions parent/child relationships, object/sylbing relationships, searching
    /// and even a mutation. These test are long, complex and multi-steped and act more as integration tests than unit tests.
    /// </para>
    /// <para>
    /// These tests only test the "happy" path, it is built assuming the developer has implemented their controllers and models correctly. It serves as a
    /// a more complex sanity check during the unit testing process.
    /// </para>
    /// <para>
    /// These tests will attempt to exercise:
    /// Queries
    /// Mutations
    /// Variables
    /// Directives
    /// Batch Queries
    /// Invalid Data error Conditions
    /// Model state validation.
    /// Type Extensions on Controllers
    /// Property Field Resolvers
    /// Object Method Field Resolvers
    /// InputValues as scalars
    /// InputValues as objects
    /// Enum Types
    /// Unions on Searches.
    /// Security checks for users.
    /// </para>
    /// <para>Result testing for all these tests will be done in a "black box" manner comparing raw json output to an expected output read from a file.</para>
    /// </summary>
    [TestFixture]
    public class MusicIndustryTests
    {
        private string LoadOutputFile(string fileName)
        {
            var directory = new FileInfo(typeof(MusicIndustryTests).Assembly.Location).Directory?.FullName;
            var path = Path.Combine(directory, "Integration", "ExpectedOutput", fileName);
            return File.ReadAllText(path);
        }

        [Test]
        public async Task EndToEndIntegrationTest()
        {
            var builder = new TestServerBuilder();
            builder.AddScoped<IMusicService, MusicService>();
            builder.AddSingleton<IMusicRepository, MusicRepository>();
            builder.AddGraphType<MusicController>();
            builder.AddGraphQL(o =>
            {
                o.ExecutionOptions.AwaitEachRequestedField = true;
            });

            var server = builder.Build();

            // Test the following areas
            // -----------------------------------
            // queries
            // input values: scalars
            // batch child query ("records" is a batch list extension method)
            // root level controller actions (note no nesting under the declared "music" field of the controller for the "artists" field)
            var expected = LoadOutputFile("RetrieveArtistsAndRecords.json");
            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(
                            @"query {
                                artists(searchText: ""queen"") {
                                    id
                                    name
                                    records {
                                        id
                                        name
                                        genre {
                                            id
                                            name
                                        }
                                    }
                                }
                            }");

            var result1 = await server.RenderResult(queryBuilder);
            CommonAssertions.AreEqualJsonStrings(expected, result1, "(1): " + result1);

            // Test the following areas
            // -----------------------------------
            // virtual routing (the "music" field is a virtual field declared on the controller)
            // mutations
            // input values: scalars
            // batch sybling query ("company" is a batch sybling extension method)
            expected = LoadOutputFile("CreateNewArtist.json");
            queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(
                        @"mutation {
                          music{
                            createArtist(artistName: ""EXO"", recordCompanyId: 4) {
                                id
                                name
                                company {
                                    id
                                    name
                                }
                            }
                        }}");

            var result2 = await server.RenderResult(queryBuilder);
            CommonAssertions.AreEqualJsonStrings(expected, result2, "(2): " + result2);

            // Test the following areas
            // -----------------------------------
            // virtual routing (the "music" field is a virtual field declared on the controller)
            // mutations
            // input values: complex objects
            // unicode characters
            expected = LoadOutputFile("CreateRecord.json");
            queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText(
                        @"mutation {
                          music{
                            createRecord(artist: {id: 10, name: ""EXO"", recordCompanyId: 4},
                                         genre: {id:2, name: ""pop""},
                                         songName:""다이아몬드"") {
                                id
                                artistId
                                genre { id name }
                                name
                            }
                        }}");

            var result3 = await server.RenderResult(queryBuilder);
            CommonAssertions.AreEqualJsonStrings(expected, result3, "(3): " + result3);
        }
    }
}