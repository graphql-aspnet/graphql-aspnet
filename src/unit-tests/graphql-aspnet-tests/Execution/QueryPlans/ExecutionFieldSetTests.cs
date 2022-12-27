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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.ExecutionFieldSetTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionFieldSetTests
    {
        public static List<object[]> TEST_DATA;

        static ExecutionFieldSetTests()
        {
            TEST_DATA = new List<object[]>();

            // for a given query, the aliases of fields in the "retrieveGame" field set should be
            // the given values in the given order
            TEST_DATA.Add(new object[]
            {
                "NoFragmentsNoAliasedFields",
                @"
                {
                    retrieveGame(id: 3) {
                        id
                        name
                        gameType
                        relatedGames {
                            id
                            name
                        }
                    }
                }
                ",
                new string[] { "id", "name", "gameType", "relatedGames" },
            });

            TEST_DATA.Add(new object[]
            {
                "InlineFragment",
                @"
                {
                    retrieveGame(id: 3) {
                        id
                        ... {
                            name
                            gameType
                        }
                        RG: relatedGames {
                            id
                            name
                        }
                    }
                }
                ",
                new string[] { "id", "name", "gameType", "RG" },
            });

            TEST_DATA.Add(new object[]
            {
                "InlineFragmentWithSubordinateSpread",
                @"
                {
                    retrieveGame(id: 3) {
                        id
                        ... {
                            name
                            gameType
                        }
                        relatedGames {
                            ... RGames
                        }
                    }
                }
                fragment RGames on Game {
                    id
                    name
                    gametype
                }
                ",
                new string[] { "id", "name", "gameType", "relatedGames" },
            });

            TEST_DATA.Add(new object[]
            {
                "NamedSpread",
                @"
                {
                    retrieveGame(id: 3) {
                        ... GameData
                    }
                }
                fragment GameData on Game {
                    id
                    name
                    gameType
                }
                ",
                new string[] { "id", "name", "gameType" },
            });

            TEST_DATA.Add(new object[]
            {
                "NestedNamedSpreads",
                @"
                {
                    retrieveGame(id: 3) {
                        ... GameData
                        relatedGames{
                            ... GameData
                            relatedGames {
                                ... GameData
                            }
                        }
                        id1: id
                    }
                }
                fragment GameData on Game {
                    id
                    name
                    gameType
                }
                ",
                new string[] { "id", "name", "gameType", "relatedGames", "id1" },
            });

            TEST_DATA.Add(new object[]
            {
                "FieldsNamedSpreadAndInlineFragment",
                @"
                {
                    retrieveGame(id: 3) {
                        id
                        name
                        ... GameData
                        ... {
                            gameType1: gameType
                        }
                    }
                }
                fragment GameData on Game {
                    id1: id
                    name1: name
                    gameType
                }
                ",
                new string[] { "id", "name", "id1", "name1", "gameType", "gameType1" },
            });

            TEST_DATA.Add(new object[]
            {
                "DeepInlineFragments",
                @"
                {
                    retrieveGame(id: 3) {
                        ... {
                            ... {
                                ... {
                                    ... {
                                        id
                                    }
                                    gameType1: gameType
                                }
                                name
                            }
                        }
                    }
                }
                ",
                new string[] { "id", "gameType1", "name" },
            });

            TEST_DATA.Add(new object[]
            {
                "MultipleNamedFragments",
                @"
                query {
                    retrieveAnyGame {
                            ...aData
                            ...bData
                    }
                }

                fragment aData on Game {
                    id
                    name
                }

                fragment bData on Game2 {
                    name
                    gameType
                }",
                new string[] { "id", "name", "name", "gameType" },
            });
        }

        [TestCaseSource(nameof(TEST_DATA))]
        public void ExecutionFieldSetExpectedResults(
            string testName,
            string queryText,
            string[] expectedAliases)
        {
            var server = new TestServerBuilder()
                .AddGraphController<GameController>()
                .Build();

            var doc = server.CreateDocument(queryText);

            var executableFieldSet = Enumerable.OfType<IFieldDocumentPart>(doc.Operations[0]
                    .FieldSelectionSet.Children[DocumentPartType.Field]).Single()
                .FieldSelectionSet.ExecutableFields;

            Assert.IsNotNull(executableFieldSet);

            var foundFields = Enumerable.ToList(executableFieldSet);
            Assert.AreEqual(expectedAliases.Length, foundFields.Count);

            for (var i = 0; i < foundFields.Count; i++)
            {
                Assert.AreEqual(expectedAliases[i], foundFields[i].Alias.ToString());
                i++;
            }
        }
    }
}