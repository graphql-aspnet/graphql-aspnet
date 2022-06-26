// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentPartsNew;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.PlanGeneration.DocumentConstructionTestData;
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
            var syntaxTree = new GraphQLParser().ParseQueryDocument(text.AsMemory());

            var document = generator.CreateDocument(syntaxTree);

            var spread = document.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single().FieldSelectionSet
                .Children.OfType<IFragmentSpreadDocumentPart>().Single();

            var fragment = document.NamedFragments[0];

            Assert.AreEqual("myFrag", spread.FragmentName.ToString());
            Assert.AreEqual(fragment, spread.Fragment);
            Assert.IsTrue(fragment.IsReferenced);
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
            var syntaxTree = new GraphQLParser().ParseQueryDocument(text.AsMemory());

            var document = generator.CreateDocument(syntaxTree);

            var spread = document.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single().FieldSelectionSet
                .Children.OfType<IFragmentSpreadDocumentPart>().Single();

            var fragment = document.NamedFragments[0];

            Assert.AreEqual("myOtherFrag", spread.FragmentName.ToString());
            Assert.IsNull(spread.Fragment);

            Assert.AreEqual("myFrag", fragment.Name);
            Assert.IsFalse(fragment.IsReferenced);
        }
    }
}