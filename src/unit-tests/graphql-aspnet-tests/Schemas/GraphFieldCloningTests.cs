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
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.AspNetCore.Authorization;
    using Moq;
    using NUnit.Framework;

    [AllowAnonymous]
    [TestFixture]
    public class GraphFieldCloningTests
    {
        [Test]
        public void MethodField_PropertyCheck()
        {
            var originalParent = new Mock<IGraphType>();
            originalParent.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/JohnType"));
            originalParent.Setup(x => x.Name).Returns("JohnType");

            var resolver = new Mock<IGraphFieldResolver>();
            var polices = new List<AppliedSecurityPolicyGroup>();
            polices.Add(AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(GraphFieldCloningTests)));

            var appliedDirectives = new AppliedDirectiveCollection();
            appliedDirectives.Add(new AppliedDirective("someDirective", 3));

            var field = new MethodGraphField(
                "field1",
                GraphTypeExpression.FromDeclaration("[Int]"),
                new SchemaItemPath("[type]/JohnType/field1"),
                "internalFieldName",
                typeof(TwoPropertyObject),
                typeof(List<TwoPropertyObject>),
                AspNet.Execution.FieldResolutionMode.PerSourceItem,
                resolver.Object,
                polices,
                appliedDirectives);

            field.AssignParent(originalParent.Object);

            field.Arguments.AddArgument(new GraphFieldArgument(
                field,
                "arg1",
                GraphTypeExpression.FromDeclaration("String"),
                field.Route.CreateChild("arg1"),
                "arg1",
                "arg1",
                typeof(string),
                false));

            field.Complexity = 1.3f;
            field.IsDeprecated = true;
            field.DeprecationReason = "Because I said so";
            field.Publish = false;
            field.FieldSource = GraphFieldSource.Method;

            var clonedParent = new Mock<IGraphType>();
            clonedParent.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/BobType"));
            clonedParent.Setup(x => x.Name).Returns("BobType");
            var clonedField = field.Clone(clonedParent.Object);

            Assert.AreEqual(field.Name, clonedField.Name);
            Assert.AreEqual(field.ObjectType, clonedField.ObjectType);
            Assert.AreEqual(field.DeclaredReturnType, clonedField.DeclaredReturnType);
            Assert.AreEqual(clonedField.TypeExpression.ToString(), clonedField.TypeExpression.ToString());
            Assert.AreEqual(field.Description, clonedField.Description);
            Assert.AreEqual(field.Publish, clonedField.Publish);
            Assert.AreEqual("[type]/BobType/field1", clonedField.Route.Path);
            Assert.AreEqual(field.Mode, clonedField.Mode);
            Assert.AreEqual(field.IsDeprecated, clonedField.IsDeprecated);
            Assert.AreEqual(field.DeprecationReason, clonedField.DeprecationReason);
            Assert.AreEqual(field.Complexity, clonedField.Complexity);
            Assert.AreEqual(field.FieldSource, clonedField.FieldSource);
            Assert.AreEqual(field.InternalFullName, clonedField.InternalFullName);

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

        [Test]
        public void PropertyField_PropertyCheck()
        {
            var originalParent = new Mock<IGraphType>();
            originalParent.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/JohnType"));
            originalParent.Setup(x => x.Name).Returns("JohnType");

            var resolver = new Mock<IGraphFieldResolver>();
            var polices = new List<AppliedSecurityPolicyGroup>();
            polices.Add(AppliedSecurityPolicyGroup.FromAttributeCollection(typeof(GraphFieldCloningTests)));

            var appliedDirectives = new AppliedDirectiveCollection();
            appliedDirectives.Add(new AppliedDirective("someDirective", 3));

            var field = new PropertyGraphField(
                "field1",
                GraphTypeExpression.FromDeclaration("[Int]"),
                new SchemaItemPath("[type]/JohnType/field1"),
                "Prop1",
                typeof(TwoPropertyObject),
                typeof(List<TwoPropertyObject>),
                AspNet.Execution.FieldResolutionMode.PerSourceItem,
                resolver.Object,
                polices,
                appliedDirectives);

            field.AssignParent(originalParent.Object);

            field.Arguments.AddArgument(new GraphFieldArgument(
                field,
                "arg1",
                GraphTypeExpression.FromDeclaration("String"),
                field.Route.CreateChild("arg1"),
                "arg1",
                "arg1",
                typeof(string),
                false));

            field.Complexity = 1.3f;
            field.IsDeprecated = true;
            field.DeprecationReason = "Because I said so";
            field.Publish = false;
            field.FieldSource = AspNet.Internal.TypeTemplates.GraphFieldSource.Method;

            var clonedParent = new Mock<IGraphType>();
            clonedParent.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/BobType"));
            clonedParent.Setup(x => x.Name).Returns("BobType");
            var clonedField = field.Clone(clonedParent.Object) as PropertyGraphField;

            Assert.IsNotNull(clonedField);
            Assert.AreEqual(field.InternalFullName, clonedField.InternalFullName);
            Assert.AreEqual(field.Name, clonedField.Name);
            Assert.AreEqual(field.ObjectType, clonedField.ObjectType);
            Assert.AreEqual(field.DeclaredReturnType, clonedField.DeclaredReturnType);
            Assert.AreEqual(clonedField.TypeExpression.ToString(), clonedField.TypeExpression.ToString());
            Assert.AreEqual(field.Description, clonedField.Description);
            Assert.AreEqual(field.Publish, clonedField.Publish);
            Assert.AreEqual("[type]/BobType/field1", clonedField.Route.Path);
            Assert.AreEqual(field.Mode, clonedField.Mode);
            Assert.AreEqual(field.IsDeprecated, clonedField.IsDeprecated);
            Assert.AreEqual(field.DeprecationReason, clonedField.DeprecationReason);
            Assert.AreEqual(field.Complexity, clonedField.Complexity);
            Assert.AreEqual(field.FieldSource, clonedField.FieldSource);

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