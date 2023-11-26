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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using Microsoft.AspNetCore.Authorization;
    using NSubstitute;
    using NUnit.Framework;

    [AllowAnonymous]
    [TestFixture]
    public class GraphFieldCloningTests
    {
        [Test]
        public void MethodField_PropertyCheck()
        {
            var originalParent = Substitute.For<IGraphType>();
            originalParent.ItemPath.Returns(new ItemPath("[type]/JohnType"));
            originalParent.Name.Returns("JohnType");

            var resolver = Substitute.For<IGraphFieldResolver>();
            var polices = new List<AppliedSecurityPolicyGroup>();
            polices.Add(AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(GraphFieldCloningTests)));

            var appliedDirectives = new AppliedDirectiveCollection();
            appliedDirectives.Add(new AppliedDirective("someDirective", 3));

            var field = new MethodGraphField(
                "field1",
                "internalFieldName",
                GraphTypeExpression.FromDeclaration("[Int]"),
                new ItemPath("[type]/JohnType/field1"),
                typeof(List<TwoPropertyObject>),
                typeof(TwoPropertyObject),
                AspNet.Execution.FieldResolutionMode.PerSourceItem,
                resolver,
                polices,
                appliedDirectives);

            field = field.Clone(originalParent) as MethodGraphField;

            field.Arguments.AddArgument(new GraphFieldArgument(
                field,
                "arg1",
                "arg1",
                "arg1",
                GraphTypeExpression.FromDeclaration("String"),
                field.ItemPath.CreateChild("arg1"),
                typeof(string),
                false));

            field.Complexity = 1.3f;
            field.IsDeprecated = true;
            field.DeprecationReason = "Because I said so";
            field.Publish = false;
            field.FieldSource = GraphFieldSource.Method;

            var clonedParent = Substitute.For<IGraphType>();
            clonedParent.ItemPath.Returns(new ItemPath("[type]/BobType"));
            clonedParent.Name.Returns("BobType");
            var clonedField = field.Clone(clonedParent);

            Assert.AreEqual(field.Name, clonedField.Name);
            Assert.AreEqual(field.ObjectType, clonedField.ObjectType);
            Assert.AreEqual(field.DeclaredReturnType, clonedField.DeclaredReturnType);
            Assert.AreEqual(clonedField.TypeExpression.ToString(), clonedField.TypeExpression.ToString());
            Assert.AreEqual(field.Description, clonedField.Description);
            Assert.AreEqual(field.Publish, clonedField.Publish);
            Assert.AreEqual("[type]/BobType/field1", clonedField.ItemPath.Path);
            Assert.AreEqual(field.Mode, clonedField.Mode);
            Assert.AreEqual(field.IsDeprecated, clonedField.IsDeprecated);
            Assert.AreEqual(field.DeprecationReason, clonedField.DeprecationReason);
            Assert.AreEqual(field.Complexity, clonedField.Complexity);
            Assert.AreEqual(field.FieldSource, clonedField.FieldSource);
            Assert.AreEqual(field.InternalName, clonedField.InternalName);

            Assert.IsFalse(object.ReferenceEquals(field.TypeExpression, clonedField.TypeExpression));
            Assert.IsTrue(object.ReferenceEquals(field.Resolver, clonedField.Resolver));
            Assert.IsFalse(object.ReferenceEquals(field.AppliedDirectives, clonedField.AppliedDirectives));
            Assert.IsFalse(object.ReferenceEquals(field.SecurityGroups, clonedField.SecurityGroups));
            Assert.IsFalse(object.ReferenceEquals(field.Arguments, clonedField.Arguments));

            Assert.AreEqual(field.AppliedDirectives.Count, clonedField.AppliedDirectives.Count);
            Assert.AreEqual(field.SecurityGroups.Count(), clonedField.SecurityGroups.Count());
            Assert.AreEqual(field.Arguments.Count, clonedField.Arguments.Count);

            foreach (var arg in field.Arguments)
            {
                var foundArg = clonedField.Arguments.FindArgument(arg.Name);
                Assert.IsNotNull(foundArg);
            }
        }
    }
}