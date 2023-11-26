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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class GraphFieldArgumentCloningTests
    {
        [Test]
        public void ClonedArgument_PropertyCheck()
        {
            var directives = new AppliedDirectiveCollection();
            directives.Add(new AppliedDirective("directive1", 3));

            var parentField = Substitute.For<ISchemaItem>();
            parentField.ItemPath.Returns(new ItemPath("[type]/GraphType1/Field1"));
            parentField.Name.Returns("Field1");

            var arg = new GraphFieldArgument(
                parentField,
                "argName",
                "internalName",
                "paramName",
                GraphTypeExpression.FromDeclaration("String"),
                new ItemPath("[type]/GraphType1/Field1/Arg1"),
                typeof(string),
                true,
                "default value",
                "a description",
                directives);

            var newParentField = Substitute.For<ISchemaItem>();
            newParentField.ItemPath.Returns(new ItemPath("[type]/GraphType2/Field1"));
            newParentField.Name.Returns("Field1");

            var clonedArg = arg.Clone(newParentField) as GraphFieldArgument;

            Assert.AreEqual(arg.Name, clonedArg.Name);
            Assert.AreEqual(arg.Description, clonedArg.Description);
            Assert.AreEqual(arg.DefaultValue, clonedArg.DefaultValue);
            Assert.AreEqual(arg.ObjectType, clonedArg.ObjectType);
            Assert.AreEqual(arg.InternalName, clonedArg.InternalName);
            Assert.AreEqual(arg.TypeExpression, clonedArg.TypeExpression);
            Assert.AreEqual(arg.ParameterName, clonedArg.ParameterName);
            Assert.AreEqual(arg.AppliedDirectives.Count, arg.AppliedDirectives.Count);

            Assert.IsFalse(object.ReferenceEquals(arg.AppliedDirectives, clonedArg.AppliedDirectives));
            Assert.IsFalse(object.ReferenceEquals(arg.Parent, clonedArg.Parent));
            Assert.IsFalse(object.ReferenceEquals(arg.TypeExpression, clonedArg.TypeExpression));
        }
    }
}