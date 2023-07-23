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
namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ParameterTestData
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using Microsoft.AspNetCore.Mvc;

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
            [FromGraphQL(TypeExpression = "Type!")] Person objectArgNotNull,
            IEnumerable<int> enumerableIntArg,
            IEnumerable<int?> enumerableIntArgWithNullableItemButNoDefault,
            [Description("This Graph Field is Amazing")][FromGraphQL("validArgNameOverride1", TypeExpression = "[Type]!")] IEnumerable<Person> __lotsOfAttributes,
            [FromGraphQL(TypeExpression = "[Type]!")] IEnumerable<int> enumerableWithNonNullableArg, // int cant be null
            [FromGraphQL(TypeExpression = "[Type!]!")] IEnumerable<int> enumerableIntArgWithAttribForbidsNullItems,
            Person[] arrayOfObjects,
            [ApplyDirective(typeof(DirectiveWithArgs), 77, "param arg")] int paramDirective,
            IEnumerable<Person>[] arrayOfEnumerableOfObject,
            IEnumerable<Person[]> enumerableOfArrayOfObjects,
            IEnumerable<Person[]>[] arrayOfEnumerableOfArrayOfObjects,
            Person[][][][][][][][][][][][][][][][][][][] deepArray,
            [FromGraphQL(TypeExpression = "[Type!")] int invalidTypeExpression,
            [GraphSkip] Person graphSkipArgument,
            ObjectWithGraphSkip typeHasGraphSkip,
            [FromGraphQL(TypeExpression = "Type!")] int? compatiableTypeExpressionSingle, // add more specificity
            [FromGraphQL(TypeExpression = "[Type!]!")] int?[] compatiableTypeExpressionList, // add more specificity
            [FromGraphQL(TypeExpression = "[Type]")] int incompatiableTypeExpressionListToSingle,
            [FromGraphQL(TypeExpression = "Type!")] int[] incompatiableTypeExpressionSingleToList,
            [FromGraphQL(TypeExpression = "Type")] int incompatiableTypeExpressionNullToNotNull, // nullable expression, actual type is not nullable
            [FromGraphQL] TwoPropertyObject justFromGraphQLDeclaration,
            [FromServices] TwoPropertyObject justFromServicesDeclaration,
            [FromGraphQL] [FromServices] TwoPropertyObject doubleDeclaredObject,
            [FromGraphQL(InternalName = "customInternalName_38")] int internalNameObject,
            Person defaultValueObjectArg = null,
            string defaultValueStringArg = null,
            string defaultValueStringArgWithValue = "abc",
            int? defaultValueNullableIntArg = 5)
        {
            return 0;
        }
    }
}