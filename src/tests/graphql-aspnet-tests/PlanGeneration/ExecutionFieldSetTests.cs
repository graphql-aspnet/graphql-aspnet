namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.PlanGeneration.ExecutionFieldSetTestData;
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
                        gametype
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
                            gametype
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
                            gametype
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
                    gametype
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
                    gametype
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
                    gametype
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
                new string[] { "id", "gameType1", "name"},
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

            var executableFieldSet = doc.Operations[0]
                .FieldSelectionSet.Children[DocumentPartType.Field].OfType<IFieldDocumentPart>().Single()
                .FieldSelectionSet.ExecutableFields;

            Assert.IsNotNull(executableFieldSet);
            Assert.AreEqual(expectedAliases.Length, executableFieldSet.Count);

            for (var i = 0; i < executableFieldSet.Count; i++)
            {
                var executableField = executableFieldSet[i];

                Assert.AreEqual(expectedAliases[i], executableField.Alias.ToString());
            }
        }
    }
}
