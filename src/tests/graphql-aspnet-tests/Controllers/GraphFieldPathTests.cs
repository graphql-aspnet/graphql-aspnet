// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers
{
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas.Structural;
    using NUnit.Framework;

    [TestFixture]
    public class GraphFieldPathTests
    {
        [TestCase(GraphCollection.Query, "path1", "path2", "[query]/path1/path2")]
        [TestCase(GraphCollection.Types, "path1", "path2", "[type]/path1/path2")]
        [TestCase(GraphCollection.Mutation, "path1", "path2", "[mutation]/path1/path2")]
        [TestCase(GraphCollection.Subscription, "path1", "path2", "[sub]/path1/path2")]
        [TestCase(GraphCollection.Unknown, "path1", "path2", "[noop]/path1/path2")]
        public void Join_WithRoot_JoinsAsExpected(GraphCollection root, string leftSide, string rightSide, string expectedOutput)
        {
            // standard join
            var fragment = GraphFieldPath.Join(root, leftSide, rightSide);
            Assert.AreEqual(expectedOutput, fragment);
        }

        [TestCase("path1", "path2", "path1/path2")]
        [TestCase("path1", "path2/path3", "path1/path2/path3")]
        public void Join_WithNoRoot_JoinsAsExpected(string leftSide, string rightSide, string expectedOutput)
        {
            var fragment = GraphFieldPath.Join(leftSide, rightSide);
            Assert.AreEqual(expectedOutput, fragment);
        }

        [TestCase("path1/path2", "path1/path2")]
        [TestCase("path1/path2/", "path1/path2")]
        [TestCase("/path1/path2", "path1/path2")]
        [TestCase("/path1/path2/", "path1/path2")]
        [TestCase("path1//path2", "path1/path2")]
        [TestCase("path1\\path2", "path1/path2")]
        [TestCase(@"path1\path2\path3", "path1/path2/path3")]
        [TestCase("///////path1///path2/path3/   ", "path1/path2/path3")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void NormalizeFragment_CleansUpAsExpected(string input, string expectedOutput)
        {
            var fragment = GraphFieldPath.NormalizeFragment(input);
            Assert.AreEqual(expectedOutput, fragment);
        }

        [TestCase("[query]/path1/path2", false)]
        [TestCase("[query]/path2", true)]
        [TestCase("[mutation]/path1/path2", false)]
        [TestCase("[mutation]/path2", true)]
        [TestCase("[noop]/path2", false)]
        [TestCase("path1/path2", false)]
        public void IsTopLevelField(string input, bool isTopField)
        {
            var fragment = new GraphFieldPath(input);
            Assert.AreEqual(isTopField, fragment.IsTopLevelField);
        }

        [Test]
        public void Destructuring_Query_TwoFragmentPathHasADefinedParent()
        {
            var fragment = "[query]/path1/path2";
            var route = new GraphFieldPath(fragment);

            // valid path should be untouched
            Assert.IsTrue(route.IsValid);
            Assert.AreEqual(fragment, route.Raw);
            Assert.AreEqual(fragment, route.Path);
            Assert.IsNotNull(route.Parent);
            Assert.AreEqual(GraphCollection.Query, route.RootCollection);
            Assert.AreEqual("path2", route.Name);
            Assert.AreEqual("path1", route.Parent.Name);
            Assert.AreEqual("[query]/path1", route.Parent.Path);
        }

        [Test]
        public void GenerateParentPathSegments_LeafPathReturnsParentList()
        {
            var fragment = "[query]/path1/path2/path3/path4";
            var route = new GraphFieldPath(fragment);

            var parents = route.GenerateParentPathSegments();
            Assert.IsNotNull(parents);
            Assert.AreEqual(3, parents.Count);

            Assert.AreEqual("[query]/path1", parents[0].Path);
            Assert.AreEqual("[query]/path1/path2", parents[1].Path);
            Assert.AreEqual("[query]/path1/path2/path3", parents[2].Path);
        }

        [Test]
        public void GenerateParentPathSegments_TopLevelFieldReturnsEmptyList()
        {
            var fragment = "[query]/path1";
            var route = new GraphFieldPath(fragment);
            Assert.IsTrue(route.IsTopLevelField);

            var parents = route.GenerateParentPathSegments();
            Assert.IsNotNull(parents);
            Assert.AreEqual(0, parents.Count);
        }

        [Test]
        public void GenerateParentPathSegments_InvalidPathSegmentReturnsEmptyList()
        {
            var fragment = "pat!$#@%h1";
            var route = new GraphFieldPath(fragment);
            Assert.IsFalse(route.IsValid);

            var parents = route.GenerateParentPathSegments();
            Assert.IsNotNull(parents);
            Assert.AreEqual(0, parents.Count);
        }

        [TestCase("[mutation]/path1/path2", "[mutation]/path1/path2", true, GraphCollection.Mutation, "path2", true, "[mutation]/path1")]
        [TestCase("[mutation]/path1///\\path2", "[mutation]/path1/path2", true, GraphCollection.Mutation, "path2", true, "[mutation]/path1")]
        [TestCase("[query]/pat$!h1/path2", "", false, GraphCollection.Query, "", false, "")]
        [TestCase("/path1/path2", "path1/path2", true, GraphCollection.Unknown, "path2", true, "path1")]
        public void Destructuring(
            string rawPath,
            string expectedPath,
            bool expectedValidState,
            GraphCollection expectedRoot,
            string expectedName,
            bool shouldHaveParent,
            string expectedParentPath)
        {
            var route = new GraphFieldPath(rawPath);

            // valid path should be untouched
            Assert.AreEqual(expectedValidState, route.IsValid);
            Assert.AreEqual(rawPath, route.Raw);
            Assert.AreEqual(expectedPath, route.Path);
            Assert.AreEqual(expectedRoot, route.RootCollection);
            Assert.AreEqual(expectedName, route.Name);

            if (!shouldHaveParent)
            {
                Assert.IsNull(route.Parent);
            }
            else
            {
                Assert.IsNotNull(route.Parent);
                Assert.AreEqual(expectedParentPath, route.Parent.Path);
            }
        }

        [TestCase("[query]/path1/path2", "[query]/path1/path2", true)]
        [TestCase("[query]/path1/path2", "[query]/path1/path2/", true)]
        [TestCase("/path1/path2", "path1/path2", true)]
        [TestCase("/path1\\path2", "path1/path2", true)]
        [TestCase("", "", false)]
        [TestCase("", null, false)]
        [TestCase("[query]/path1/path2", "[query]/path1\\PATH2", false)]
        [TestCase("[mutation]/path1/path2", "[query]/path1/path2", false)]
        [TestCase("[query]/path1/path2", "[query]/path1/path2/path3", false)]
        [TestCase("[query]/path1/path2", "path1/path2", false)]
        public void IsSameRoute(string fragment1, string fragment2, bool areTheSame)
        {
            var route1 = new GraphFieldPath(fragment1);
            var route2 = new GraphFieldPath(fragment2);

            // valid path should be untouched
            Assert.AreEqual(areTheSame, route1.IsSameRoute(route2));
        }

        [TestCase("[query]/path1/path2", "[query]/path1/path2/path3", true)]
        [TestCase("[mutation]/path1/path2", "[mutation]/path1/path2/path3", true)]
        [TestCase("[query]/path1/path2", "[mutation]/path1/path2/path3", false)]
        [TestCase("[mutation]/path1/path2", "[query]/path1/path2/path3", false)]
        [TestCase("path1/path2", "path1/path2/path3", true)]
        [TestCase("", "", false)]
        [TestCase("[query]/path1/path2", "[query]/path1/pathQ/path3", false)]
        public void HasChildRoute(string fragment1, string fragment2, bool frag2IsChildof1)
        {
            var route = new GraphFieldPath(fragment1);
            var route2 = new GraphFieldPath(fragment2);

            // route2 is a child of route 1, but not the other way around
            Assert.AreEqual(frag2IsChildof1, route.HasChildRoute(route2));
        }

        [Test]
        public void FromConstructorParts_YieldsCombinedPaths()
        {
            var route = new GraphFieldPath(GraphCollection.Types, "typeName", "fieldName");
            Assert.AreEqual($"{Constants.Routing.TYPE_ROOT}/typeName/fieldName", route.Path);
        }

        [Test]
        public void GraphRouteArgumentPath_YieldsAlternatePathString()
        {
            var parent = new GraphFieldPath($"{Constants.Routing.TYPE_ROOT}/typeName/fieldName");
            var route = new GraphArgumentFieldPath(parent, "arg1");
            Assert.AreEqual($"{Constants.Routing.TYPE_ROOT}/typeName/fieldName[arg1]", route.Path);
        }
    }
}