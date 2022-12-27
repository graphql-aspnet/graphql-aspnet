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
    using System.Collections.Generic;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.ExecutionFieldSetTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class ExeceutableFieldSelectionSetEnumeratorTests
    {
        private readonly TestServer<GraphSchema> _server;

        public ExeceutableFieldSelectionSetEnumeratorTests()
        {
            _server = new TestServerBuilder()
                .AddGraphController<GameController>()
                .AddDirective<IncludeDirective>()
                .AddDirective<SkipDirective>()
                .Build();
        }

        private IQueryDocument CreateDocument(string text)
        {
            var generator = new DefaultQueryDocumentGenerator<GraphSchema>(_server.Schema);
            return generator.CreateDocument(text.AsSpan());
        }

        private void MarkPartOrChildPartAsNotIncluded(IDocumentPart part, Func<IIncludeableDocumentPart, bool> predicate)
        {
            if (part is IIncludeableDocumentPart idp && predicate(idp))
                idp.IsIncluded = false;

            foreach (var child in part.Children)
            {
                this.MarkPartOrChildPartAsNotIncluded(child, predicate);
            }
        }

        private int IterateThroughFieldSelectionSet(
            IFieldSelectionSetDocumentPart fieldSelectionSet,
            bool iterateIncludedFieldsOnly,
            List<string> expectedFields,
            int expectedFieldIndex)
        {
            var enumerator = new ExecutableFieldSelectionSetEnumerator(fieldSelectionSet, iterateIncludedFieldsOnly);
            while (enumerator.MoveNext())
            {
                var field = enumerator.Current;
                Assert.AreEqual(expectedFields[expectedFieldIndex], field.Name);
                expectedFieldIndex++;

                if (field is IFieldSelectionSetDocumentPart fieldSet)
                {
                    expectedFieldIndex = this.IterateThroughFieldSelectionSet(
                        fieldSet,
                        iterateIncludedFieldsOnly,
                        expectedFields,
                        expectedFieldIndex);
                }
            }

            return expectedFieldIndex;
        }

        private void ExecuteTest(
            string text,
            Func<IDocumentPart, bool> excludePredicate,
            bool iterateIncludedFieldsOnly,
            List<string> expectedFields)
        {
            var document = this.CreateDocument(text);
            var operation = document.Operations[0];

            this.MarkPartOrChildPartAsNotIncluded(operation, excludePredicate);
            this.IterateThroughFieldSelectionSet(operation.FieldSelectionSet, iterateIncludedFieldsOnly, expectedFields, 0);
        }

        [TestCase(true, new string[] { "game", "retrieveGame", "id", "name" })]
        [TestCase(false, new string[] { "game", "retrieveGame", "id", "name" })]
        public void NoMarkableFields(bool includedOnly, string[] expectedFields)
        {
            var text = @"query {
                game {
                    retrieveGame(int: 5) {
                        id
                        name
                    }
                }
            }";

            this.ExecuteTest(
                text,
                x => false,
                includedOnly,
                new List<string>(expectedFields));
        }

        [TestCase(true, new string[] { "game", "retrieveGame", "name" })]
        [TestCase(false, new string[] { "game", "retrieveGame", "id", "name" })]
        public void LeafField(bool includedOnly, string[] expectedFields)
        {
            var text = @"query {
                game {
                    retrieveGame(int: 5) {
                        id
                        name
                    }
                }
            }";

            this.ExecuteTest(
                text,
                x => x is IFieldDocumentPart fdp && fdp.Name == "id",
                includedOnly,
                new List<string>(expectedFields));
        }

        [TestCase(true, new string[] { "game" })]
        [TestCase(false, new string[] { "game", "retrieveGame", "id", "name" })]
        public void MidLevelField(bool includedOnly, string[] expectedFields)
        {
            var text = @"query {
                game {
                    retrieveGame(int: 5) {
                        id
                        name
                    }
                }
            }";

            this.ExecuteTest(
                text,
                x => x is IFieldDocumentPart fdp && fdp.Name == "retrieveGame",
                includedOnly,
                new List<string>(expectedFields));
        }

        [TestCase(true, new string[] { "game", "retrieveGame", "id" })]
        [TestCase(false, new string[] { "game", "retrieveGame", "id", "name" })]
        public void InlineFragment(bool includedOnly, string[] expectedFields)
        {
            var text = @"query {
                game {
                    retrieveGame(int: 5) {
                        id
                        ... {
                            name
                        }
                    }
                }
            }";

            this.ExecuteTest(
                text,
                x => x is IInlineFragmentDocumentPart,
                includedOnly,
                new List<string>(expectedFields));
        }

        [TestCase(true, new string[] { "game", "retrieveGame", "id" })]
        [TestCase(false, new string[] { "game", "retrieveGame", "id", "name" })]
        public void FragmentSpread(bool includedOnly, string[] expectedFields)
        {
            var text = @"query {
                game {
                    retrieveGame(int: 5) {
                        id
                        ... frag1
                    }
                }
            }
            fragment frag1 on Game {
                name
            }";

            this.ExecuteTest(
                text,
                x => x is IFragmentSpreadDocumentPart,
                includedOnly,
                new List<string>(expectedFields));
        }
    }
}