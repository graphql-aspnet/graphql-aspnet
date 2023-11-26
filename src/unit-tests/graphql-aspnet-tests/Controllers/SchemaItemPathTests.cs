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
    public class SchemaItemPathTests
    {
        [TestCase(ItemPathRoots.Query, "path1", "path2", "[query]/path1/path2")]
        [TestCase(ItemPathRoots.Types, "path1", "path2", "[type]/path1/path2")]
        [TestCase(ItemPathRoots.Mutation, "path1", "path2", "[mutation]/path1/path2")]
        [TestCase(ItemPathRoots.Subscription, "path1", "path2", "[subscription]/path1/path2")]
        [TestCase(ItemPathRoots.Unknown, "path1", "path2", "[noop]/path1/path2")]
        public void Join_WithRoot_JoinsAsExpected(ItemPathRoots root, string leftSide, string rightSide, string expectedOutput)
        {
            // standard join
            var fragment = ItemPath.Join(root, leftSide, rightSide);
            Assert.AreEqual(expectedOutput, fragment);
        }

        [TestCase("path1", "path2", "path1/path2")]
        [TestCase("path1", "path2/path3", "path1/path2/path3")]
        public void Join_WithNoRoot_JoinsAsExpected(string leftSide, string rightSide, string expectedOutput)
        {
            var fragment = ItemPath.Join(leftSide, rightSide);
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
            var fragment = ItemPath.NormalizeFragment(input);
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
            var fragment = new ItemPath(input);
            Assert.AreEqual(isTopField, fragment.IsTopLevelField);
        }

        [Test]
        public void Query_TwoFragmentPathHasADefinedParent()
        {
            var fragment = "[query]/path1/path2";
            var itemPath = new ItemPath(fragment);

            // valid path should be untouched
            Assert.IsTrue(itemPath.IsValid);
            Assert.AreEqual(fragment, itemPath.Raw);
            Assert.AreEqual(fragment, itemPath.Path);
            Assert.IsNotNull(itemPath.Parent);
            Assert.AreEqual(ItemPathRoots.Query, itemPath.Root);
            Assert.AreEqual("path2", itemPath.Name);
            Assert.AreEqual("path1", itemPath.Parent.Name);
            Assert.AreEqual("[query]/path1", itemPath.Parent.Path);
        }

        [Test]
        public void GenerateParentPathSegments_LeafPathReturnsParentList()
        {
            var fragment = "[query]/path1/path2/path3/path4";
            var itemPath = new ItemPath(fragment);

            var parents = itemPath.GenerateParentPathSegments();
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
            var itemPath = new ItemPath(fragment);
            Assert.IsTrue(itemPath.IsTopLevelField);

            var parents = itemPath.GenerateParentPathSegments();
            Assert.IsNotNull(parents);
            Assert.AreEqual(0, parents.Count);
        }

        [Test]
        public void GenerateParentPathSegments_InvalidPathSegmentReturnsEmptyList()
        {
            var fragment = "pat!$#@%h1";
            var itemPath = new ItemPath(fragment);
            Assert.IsFalse(itemPath.IsValid);

            var parents = itemPath.GenerateParentPathSegments();
            Assert.IsNotNull(parents);
            Assert.AreEqual(0, parents.Count);
        }

        [TestCase("[mutation]/path1/path2", "[mutation]/path1/path2", true, ItemPathRoots.Mutation, "path2", true, "[mutation]/path1")]
        [TestCase("[mutation]/path1///\\path2", "[mutation]/path1/path2", true, ItemPathRoots.Mutation, "path2", true, "[mutation]/path1")]
        [TestCase("[query]/pat$!h1/path2", "", false, ItemPathRoots.Query, "", false, "")]
        [TestCase("/path1/path2", "path1/path2", true, ItemPathRoots.Unknown, "path2", true, "path1")]
        public void Destructuring(
            string rawPath,
            string expectedPath,
            bool expectedValidState,
            ItemPathRoots expectedRoot,
            string expectedName,
            bool shouldHaveParent,
            string expectedParentPath)
        {
            var itemPath = new ItemPath(rawPath);

            // valid path should be untouched
            Assert.AreEqual(expectedValidState, itemPath.IsValid);
            Assert.AreEqual(rawPath, itemPath.Raw);
            Assert.AreEqual(expectedPath, itemPath.Path);
            Assert.AreEqual(expectedRoot, itemPath.Root);
            Assert.AreEqual(expectedName, itemPath.Name);

            if (!shouldHaveParent)
            {
                Assert.IsNull(itemPath.Parent);
            }
            else
            {
                Assert.IsNotNull(itemPath.Parent);
                Assert.AreEqual(expectedParentPath, itemPath.Parent.Path);
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
        public void IsSameitemPath(string fragment1, string fragment2, bool areTheSame)
        {
            var itemPath1 = new ItemPath(fragment1);
            var itemPath2 = new ItemPath(fragment2);

            // valid path should be untouched
            Assert.AreEqual(areTheSame, itemPath1.IsSamePath(itemPath2));
        }

        [TestCase("[query]/path1/path2", "[query]/path1/path2/path3", true)]
        [TestCase("[mutation]/path1/path2", "[mutation]/path1/path2/path3", true)]
        [TestCase("[query]/path1/path2", "[mutation]/path1/path2/path3", false)]
        [TestCase("[mutation]/path1/path2", "[query]/path1/path2/path3", false)]
        [TestCase("path1/path2", "path1/path2/path3", true)]
        [TestCase("", "", false)]
        [TestCase("[query]/path1/path2", "[query]/path1/pathQ/path3", false)]
        public void HasChilditemPath(string fragment1, string fragment2, bool frag2IsChildof1)
        {
            var itemPath = new ItemPath(fragment1);
            var itemPath2 = new ItemPath(fragment2);

            // itemPath2 is a child of itemPath 1, but not the other way around
            Assert.AreEqual(frag2IsChildof1, itemPath.HasChildPath(itemPath2));
        }

        [Test]
        public void FromConstructorParts_YieldsCombinedPaths()
        {
            var itemPath = new ItemPath(ItemPathRoots.Types, "typeName", "fieldName");
            Assert.AreEqual($"{Constants.Routing.TYPE_ROOT}/typeName/fieldName", itemPath.Path);
        }

        [Test]
        public void GraphitemPathArgumentPath_YieldsAlternatePathString()
        {
            var parent = new ItemPath($"{Constants.Routing.TYPE_ROOT}/typeName/fieldName");
            var itemPath = new GraphArgumentFieldPath(parent, "arg1");
            Assert.AreEqual($"{Constants.Routing.TYPE_ROOT}/typeName/fieldName[arg1]", itemPath.Path);
        }

        [TestCase("[query]/path1/path2", ItemPathRoots.Query, "/path1/path2")]
        [TestCase("[query]", ItemPathRoots.Query, "/")]
        [TestCase("[mutation]/path1/path2", ItemPathRoots.Mutation, "/path1/path2")]
        [TestCase("[subscription]/path1/path2", ItemPathRoots.Subscription, "/path1/path2")]
        [TestCase("[wrong]/path1/path2", ItemPathRoots.Unknown, "")]
        [TestCase("[query]/path1", ItemPathRoots.Query, "/path1")]
        [TestCase("[mutation]/path1", ItemPathRoots.Mutation, "/path1")]
        [TestCase("[subscription]/path1", ItemPathRoots.Subscription, "/path1")]
        [TestCase("[wrong]/path1", ItemPathRoots.Unknown, "")]
        public void Destructuring_ToCollectionAndPath(
            string input,
            ItemPathRoots expectedCollection,
            string expectedPath)
        {
            var itemPath = new ItemPath(input);
            var (col, path) = itemPath;

            Assert.AreEqual(expectedCollection, col);
            Assert.AreEqual(expectedPath, path);
        }

        [Test]
        public void TakingParent_OfType_AndMakingitemPath_IsValid()
        {
            var item = new ItemPath($"{Constants.Routing.TYPE_ROOT}/typeName");
            var newItem = item.Parent.CreateChild("otherType");

            Assert.AreEqual($"{Constants.Routing.TYPE_ROOT}/otherType", newItem.ToString());
        }

        [Test]
        public void TakingParent_OfQuery_AndMakingitemPath_IsValid()
        {
            var item = new ItemPath($"{Constants.Routing.QUERY_ROOT}/fieldName");
            var newItem = item.Parent.CreateChild("otherField");

            Assert.AreEqual($"{Constants.Routing.QUERY_ROOT}/otherField", newItem.ToString());
        }
    }
}