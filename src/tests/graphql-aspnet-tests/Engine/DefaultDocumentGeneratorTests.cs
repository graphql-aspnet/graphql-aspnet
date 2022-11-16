// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.RulesEngine.DocumentConstructionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultDocumentGeneratorTests
    {
        [Test]
        public void FragmentSpreadHasAssociatedFragmentAfterGeneration()
        {
            var server = new TestServerBuilder()
                .AddGraphController<BakeryController>()
                .Build();

            var text = @"
                query {
                    retrieveById(id: 3){
                        ... myFrag
                    }
                }

                fragment myFrag on Donut {
                    id
                    name
                }";

            var generator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var source = new SourceText(text.AsSpan());
            var syntaxTree = new GraphQLParser().ParseQueryDocument(ref source);

            var document = generator.CreateDocument(source, syntaxTree);

            var spread = Enumerable.OfType<IFragmentSpreadDocumentPart>(Enumerable.OfType<IFieldDocumentPart>(document.Operations[string.Empty].FieldSelectionSet
                        .Children).Single().FieldSelectionSet
                    .Children).Single();

            var fragment = document.NamedFragments[0];

            Assert.AreEqual("myFrag", spread.FragmentName.ToString());
            Assert.AreEqual(fragment, spread.Fragment);
            Assert.IsTrue((bool)fragment.IsReferenced);
        }

        [Test]
        public void FragmentSpreadWithNoMatchingFragmentHasNulLReference()
        {
            var server = new TestServerBuilder()
                .AddGraphController<BakeryController>()
                .Build();

            var text = @"
                query {
                    retrieveById(id: 3){
                        ... myOtherFrag
                    }
                }

                fragment myFrag on Donut {
                    id
                    name
                }";

            var generator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var source = new SourceText(text.AsSpan());
            var syntaxTree = new GraphQLParser().ParseQueryDocument(ref source);

            var document = generator.CreateDocument(source, syntaxTree);

            var spread = Enumerable.OfType<IFragmentSpreadDocumentPart>(Enumerable.OfType<IFieldDocumentPart>(document.Operations[string.Empty].FieldSelectionSet
                        .Children).Single().FieldSelectionSet
                    .Children).Single();

            var fragment = document.NamedFragments[0];

            Assert.AreEqual("myOtherFrag", spread.FragmentName.ToString());
            Assert.IsNull(spread.Fragment);

            Assert.AreEqual("myFrag", fragment.Name);
            Assert.IsFalse((bool)fragment.IsReferenced);
        }

        [Test]
        public void ExecutableFieldsAreLocatedCorrectlyAcrossFragments()
        {
            var server = new TestServerBuilder()
             .AddGraphController<BakeryController>()
             .Build();

            var text = @"
                    query {
                        bakery{
                            retrieveDonut(id: 4){
                                id
                                ... {
                                    __typename
                                }
                                ... frag1
                                name
                            }
                        }
                    }

                    fragment frag1 on Donut {
                        name
                        flavor
                    }";

            var generator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var source = new SourceText(text.AsSpan());
            var syntaxTree = new GraphQLParser().ParseQueryDocument(ref source);

            var document = generator.CreateDocument(source, syntaxTree);

            var retrieveDonut = document.Operations[0]
                .FieldSelectionSet.ExecutableFields[0] // bakery
                .FieldSelectionSet.ExecutableFields[0] // retrieveDonut
                .FieldSelectionSet;

            // childen are the field, the inline frag and frag1 spread
            Assert.AreEqual(4, retrieveDonut.Children.Count);
            Assert.AreEqual(2, Enumerable.OfType<IFieldDocumentPart>(retrieveDonut.Children).Count());
            Assert.AreEqual(1, Enumerable.OfType<IInlineFragmentDocumentPart>(retrieveDonut.Children).Count());
            Assert.AreEqual(1, Enumerable.OfType<IFragmentSpreadDocumentPart>(retrieveDonut.Children).Count());

            // executable fields are the combined field set in expected order
            Assert.AreEqual(5, Enumerable.Count(retrieveDonut.ExecutableFields));
            Assert.AreEqual("id", retrieveDonut.ExecutableFields[0].Name.ToString());
            Assert.AreEqual("__typename", retrieveDonut.ExecutableFields[1].Name.ToString());
            Assert.AreEqual("name", retrieveDonut.ExecutableFields[2].Name.ToString());
            Assert.AreEqual("flavor", retrieveDonut.ExecutableFields[3].Name.ToString());
            Assert.AreEqual("name", retrieveDonut.ExecutableFields[4].Name.ToString());
        }
    }
}