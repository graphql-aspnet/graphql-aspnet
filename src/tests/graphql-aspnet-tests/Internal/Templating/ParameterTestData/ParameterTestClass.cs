// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Global
namespace GraphQL.AspNet.Tests.Internal.Templating.ParameterTestData
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;

    public class ParameterTestClass
    {
        public int TestMethod(
            string stringArg,
            string __invalidGraphNameArg,
            [Description("This argument has a description")]string argWithDescription,
            [FromGraphQL("validArgNameOveride")] string __invalidGraphNameWithOverride,
            int intArg,
            int? nullableIntArg,
            List<int> listIntArg,
            Person objectArg,
            [FromGraphQL(TypeExpressions.IsNotNull)] Person objectArgNotNull,
            IEnumerable<int> enumerableIntArg,
            IEnumerable<int?> enumerableIntArgWithNullableItemButNoDefault,
            [Description("This Graph Field is Amazing")][FromGraphQL("validArgNameOverride1", TypeExpressions.IsNotNullList)] IEnumerable<Person> __lotsOfAttributes,
            [FromGraphQL(TypeExpressions.IsNotNullList)] IEnumerable<int> enumerableWithNonNullableArg, // int cant be null
            [FromGraphQL(TypeExpressions.IsNotNull | TypeExpressions.IsNotNullList)] IEnumerable<int> enumerableIntArgWithAttribForbidsNullItems,
            Person defaultValueObjectArg = null,
            string defaultValueStringArg = null,
            string defaultValueStringArgWithValue = "abc",
            int? defaultValueNullableIntArg = 5)
        {
            return 0;
        }
    }
}