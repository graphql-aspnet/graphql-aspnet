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
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultTypeTests
    {
        [Test]
        public void Scalars_EnsureAllGlobalScalarNamesHaveAnAssociatedType()
        {
            var fields = typeof(Constants.ScalarNames)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly);

            foreach (FieldInfo fi in fields)
            {
                Assert.IsTrue(
                    GlobalTypes.IsBuiltInScalar(fi.GetRawConstantValue()?.ToString()),
                    $"The scalar name '{fi.GetRawConstantValue()}' does not exist in the {{{nameof(GlobalTypes)}}} collection.");
            }
        }
    }
}