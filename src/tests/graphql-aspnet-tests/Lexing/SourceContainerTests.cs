// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Lexing
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using NUnit.Framework;

    [TestFixture]
    public class SourceContainerTests
    {
        [TestCase(new object[0], "[]")]
        [TestCase(new object[] { "path1" }, "[\"path1\"]")]
        [TestCase(new object[] { "path1", 8, "path2" }, "[\"path1\", 8, \"path2\"]")]
        [TestCase(new object[] { 8 }, "[8]")]
        public void SourcePath_ToString(object[] items, string expectedString)
        {
            var path = new SourcePath();
            foreach (var item in items)
            {
                if (item is int i)
                    path.AddArrayIndex(i);
                else if (item is string s)
                    path.AddFieldName(s);
            }

            var result = path.ArrayString();
            Assert.AreEqual(items.Length, path.Count);
            Assert.AreEqual(expectedString, result);

            var resultArray = path.ToArray();
            Assert.AreEqual(items, resultArray);

            var origin = path.AsOrigin();
            Assert.AreEqual(path, origin.Path);
            Assert.AreEqual(SourceLocation.None, origin.Location);
        }

        [TestCase(new object[0], "[]")]
        [TestCase(new object[] { "path1" }, "[]")]
        [TestCase(new object[] { "path1", 8, "path2" }, "[\"path1\"]")]
        [TestCase(new object[] { 8 }, "[]")]
        [TestCase(new object[] { "path1", "path2", 8, "path3" }, "[\"path1\", \"path2\"]")]
        [TestCase(new object[] { "path1", "path2", 8 }, "[\"path1\", \"path2\"]")]
        public void SourcePath_MakeParent(object[] items, string expectedString)
        {
            var path = new SourcePath();
            foreach (var item in items)
            {
                if (item is int i)
                    path.AddArrayIndex(i);
                else if (item is string s)
                    path.AddFieldName(s);
            }

            var parent = path.MakeParent();
            var result = parent.ArrayString();

            Assert.AreEqual(expectedString, result);

            var origin = parent.AsOrigin();
            Assert.AreEqual(parent, origin.Path);
            Assert.AreEqual(SourceLocation.None, origin.Location);
        }

        [Test]
        public void SourcePath_EnsureNoneIsAlwaysEmpty()
        {
            var noPath = SourcePath.None;
            Assert.AreEqual(0, noPath.Count, "Source Path must be -1 to indicate no location");
        }

        [Test]
        public void SourcePath_NoPathIsNotTHeSameAsEmptyPath()
        {
            // saftey check in case operator overloads for path happen
            var noPath = SourcePath.None;
            var emptyPath = new SourcePath();
            Assert.IsTrue(noPath != emptyPath, "An empty path must not have equality to the 'None' path");
        }

        [Test]
        public void SourceLocation_EnsureNoneIsAlwaysNeg1()
        {
            var noLocation = SourceLocation.None;
            Assert.AreEqual(-1, noLocation.LineNumber, "Line number must be -1 to indicate no location");
            Assert.AreEqual(-1, noLocation.LineIndex, "Line index must be -1 to indicate no location");
            Assert.AreEqual(-1, noLocation.AbsoluteIndex, "Absolute index must be -1 to indicate no location");
        }

        [Test]
        public void SourceLocation_WithText_PropertyCheck()
        {
            var text = "test".AsMemory();

            var location = new SourceLocation(5, text, 2, 1);
            Assert.AreEqual(text, location.LineText);
            Assert.AreEqual(5, location.AbsoluteIndex);
            Assert.AreEqual(2, location.LineNumber);
            Assert.AreEqual(1, location.LineIndex);
            Assert.AreEqual(2, location.LinePosition);

            Assert.AreEqual("Line: 2, Column: 2", location.ToString());

            var origin = location.AsOrigin();
            Assert.AreEqual(location, origin.Location);
            Assert.AreEqual(SourcePath.None, origin.Path);
        }

        [Test]
        public void SourceLocation_WithNoText_PropertyCheck()
        {
            var location = new SourceLocation(5, 2, 1);
            Assert.AreEqual(ReadOnlyMemory<char>.Empty, location.LineText);
            Assert.AreEqual(5, location.AbsoluteIndex);
            Assert.AreEqual(2, location.LineNumber);
            Assert.AreEqual(1, location.LineIndex);
            Assert.AreEqual(2, location.LinePosition);
        }

        [Test]
        public void SourceOrigin_WithBothParts_PropertyCheck()
        {
            var path = new SourcePath();
            path.AddFieldName("topField");
            path.AddFieldName("middleField");
            path.AddArrayIndex(0);

            var location = new SourceLocation(5, 2, 1);

            var origin = new SourceOrigin(location, path);
            Assert.AreEqual(path, origin.Path);
            Assert.AreEqual(location, origin.Location);

            // prefer path over source text when available
            Assert.AreEqual(path.ToString(), origin.ToString());
        }

        [Test]
        public void SourceOrigin_NoLocation_PropertyCheck()
        {
            var path = new SourcePath();
            path.AddFieldName("topField");
            path.AddFieldName("middleField");
            path.AddArrayIndex(0);

            var origin = new SourceOrigin(path);
            Assert.AreEqual(path, origin.Path);
            Assert.AreEqual(SourceLocation.None, origin.Location);
        }

        [Test]
        public void SourceOrigin_NoPath_PropertyCheck()
        {
            var location = new SourceLocation(5, 2, 1);

            var origin = new SourceOrigin(location);
            Assert.AreEqual(SourcePath.None, origin.Path);
            Assert.AreEqual(location, origin.Location);

            // use location string if path is empty
            Assert.AreEqual(location.ToString(), origin.ToString());
        }

        [Test]
        public void SourceOrigin_Empty_PropertyCheck()
        {
            var origin = new SourceOrigin();
            Assert.AreEqual(SourcePath.None, origin.Path);
            Assert.AreEqual(SourceLocation.None, origin.Location);
        }
    }
}