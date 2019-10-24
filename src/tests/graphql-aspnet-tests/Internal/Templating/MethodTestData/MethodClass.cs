// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.MethodTestData
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;

    public class MethodClass
    {
        [GraphField]
        public TwoPropertyObject TestActionMethod(string arg1, int arg2)
        {
            return new TwoPropertyObject()
            {
                Property1 = arg1,
                Property2 = arg2,
            };
        }

        [GraphField(TypeExpression = TypeExpressions.IsNotNull)]
        public string ForceNotNull()
        {
            return null;
        }

        [GraphField]
        public string SimpleMethodNoAttributes()
        {
            return string.Empty;
        }

        [GraphField("SuperName")]
        public string overriddenName()
        {
            return string.Empty;
        }

        [GraphField]
        public int? ExplicitFieldTypeOptions()
        {
            return null;
        }

        [GraphField("Super!Name")]
        public string InvalidoverriddenName()
        {
            return string.Empty;
        }

        [GraphField]
        [Description("A Valid Description")]
        public string DescriptiionMethod()
        {
            return string.Empty;
        }

        [GraphField]
        [Deprecated("A Dep reason")]
        public string DepreciatedMethodWithReason()
        {
            return string.Empty;
        }

        [GraphField]
        [Deprecated]
        public string DepreciatedMethodWithNoReason()
        {
            return string.Empty;
        }

        [GraphField]
        public Task<string> AsyncMethod()
        {
            return Task.FromResult(string.Empty);
        }

        [GraphField]
        public Task AsyncMethodNoReturnType()
        {
            return Task.CompletedTask;
        }

        [GraphField]
        public Task<string> SimpleParameterCheck(int arg1)
        {
            return Task.FromResult(string.Empty);
        }

        [GraphField]
        public Task<string> ParamWithAlternateName([FromGraphQL("arg55")] int arg1)
        {
            return Task.FromResult(string.Empty);
        }

        [GraphField]
        public Task<string> InvalidParameterName([FromGraphQL("arg!55")] int arg1)
        {
            return Task.FromResult(string.Empty);
        }

        [GraphField]
        public TwoPropertyObject ObjectReturnType()
        {
            return null;
        }

        [GraphField]
        public List<TwoPropertyObject> ListOfObjectReturnType()
        {
            return null;
        }

        [GraphField]
        public Task<List<TwoPropertyObject>> TaskOfListOfObjectReturnType()
        {
            return null;
        }

        [GraphField]
        public Task<string> NullableParameter(int? arg1 = 5)
        {
            return Task.FromResult(string.Empty);
        }

        public string UnTaggedMethod()
        {
            return string.Empty;
        }

        [GraphField("path3")]
        public Task<TwoPropertyObject> EnsureMethodSignatureTestMethod(int arg1, string arg2)
        {
            return Task.FromResult(new TwoPropertyObject());
        }
    }
}