﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing.NodeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Parsing.Lexing;
    using GraphQL.AspNet.Parsing.Lexing.Exceptions;
    using GraphQL.AspNet.Parsing.Lexing.Source;
    using GraphQL.AspNet.Parsing.NodeMakers;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using NUnit.Framework;

    /// <summary>
    /// Tests dealing with the processing of a collection of single variables and the production
    /// of a <see cref="VariableCollectionNode"/>.
    /// </summary>
    [TestFixture]
    public class VariableNodeCollectionMakerTests
    {
        [Test]
        public void VariableNode_WithMultipleVariables_SetsNameCorrectly()
        {
            var test = @"($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2"")";
            var stream = Lexer.Tokenize(new SourceText(test.AsMemory()));
            stream.Prime();

            var collection = VariableCollectionNodeMaker.Instance.MakeNode(ref stream) as VariableCollectionNode;
            Assert.IsNotNull(collection);
            Assert.AreEqual(3, collection.Children.Count);

            var child = collection.Children.ElementAt(0) as VariableNode;
            Assert.IsNotNull(child);
            Assert.AreEqual("episode", child.Name.ToString());
            Assert.AreEqual("Episode", child.TypeExpression.ToString());
            var defaultValue = child.Children.FirstOrDefault<EnumValueNode>();
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("JEDI", defaultValue.Value.ToString());

            child = collection.Children.ElementAt(1) as VariableNode;
            Assert.IsNotNull(child);
            Assert.AreEqual("hero", child.Name.ToString());
            Assert.AreEqual("Hero", child.TypeExpression.ToString());
            Assert.IsNull(child.Children);

            child = collection.Children.ElementAt(2) as VariableNode;
            Assert.IsNotNull(child);
            Assert.AreEqual("droid", child.Name.ToString());
            Assert.AreEqual("Droid", child.TypeExpression.ToString());
            var defaultValue3 = child.Children.FirstOrDefault<ScalarValueNode>();
            Assert.IsNotNull(defaultValue3);
            Assert.AreEqual(ScalarValueType.String, defaultValue3.ValueType);
            Assert.AreEqual("\"R2-D2\"", defaultValue3.Value.ToString());
        }

        [Test]
        public void VariableNode_NoClosingParen_ThrowsExceptions()
        {
            var test = @"($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2""";
            var stream = Lexer.Tokenize(new SourceText(test.AsMemory()));
            stream.Prime();

            try
            {
                VariableCollectionNodeMaker.Instance.MakeNode(ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }

        [Test]
        public void VariableNode_NotPointingAtACollection_ThrowsExceptions()
        {
            var test = @"someField($episode: Episode = JEDI, $hero: Hero, $droid:Droid = ""R2-D2"")";
            var stream = Lexer.Tokenize(new SourceText(test.AsMemory()));
            stream.Prime();

            try
            {
                VariableCollectionNodeMaker.Instance.MakeNode(ref stream);
            }
            catch (GraphQLSyntaxException)
            {
                return;
            }

            Assert.Fail("Expection syntax exception");
        }
    }
}