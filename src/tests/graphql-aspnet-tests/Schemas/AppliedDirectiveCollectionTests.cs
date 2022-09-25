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
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NUnit.Framework;

    [TestFixture]
    public class AppliedDirectiveCollectionTests
    {
        [Test]
        public void DirectivesWithDifferentTypesAreAdded()
        {
            var collection = new AppliedDirectiveCollection();
            collection.Add(new AppliedDirective(typeof(DeprecatedDirective)));
            collection.Add(new AppliedDirective(typeof(SpecifiedByDirective)));

            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void AddingTheSameDirectiveTwiceFails()
        {
            var collection = new AppliedDirectiveCollection();

            var directive = new AppliedDirective(typeof(DeprecatedDirective), "Reason 1");
            collection.Add(directive);
            collection.Add(directive);

            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void DirectivesWithSameArgsAreAdded()
        {
            var collection = new AppliedDirectiveCollection();
            collection.Add(new AppliedDirective(typeof(DeprecatedDirective), "Reason 1"));
            collection.Add(new AppliedDirective(typeof(DeprecatedDirective), "Reason 1"));

            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void DirectivesWithDifferentArgsAreAdded()
        {
            var collection = new AppliedDirectiveCollection();
            collection.Add(new AppliedDirective(typeof(DeprecatedDirective), "Reason 1"));
            collection.Add(new AppliedDirective(typeof(DeprecatedDirective), "Reason 2"));

            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void DirectivesWithDifferentNamesAreAdded()
        {
            var collection = new AppliedDirectiveCollection();
            collection.Add(new AppliedDirective("deprecated"));
            collection.Add(new AppliedDirective("specifiedBy"));

            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void DirectivesOfVaryingTypesAreAdded()
        {
            var collection = new AppliedDirectiveCollection();
            collection.Add(new AppliedDirective(typeof(DeprecatedDirective)));
            collection.Add(new AppliedDirective("deprecated"));

            Assert.AreEqual(2, collection.Count);
        }
    }
}