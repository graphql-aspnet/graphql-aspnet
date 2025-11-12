// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Directives.DirectiveTestData
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class OneOfDirectiveController : GraphController
    {
        [QueryRoot(typeof(string))]
        public IGraphActionResult SubmitSingleValue(InputUnionWithOneOfDirective input)
        {
            if (input is null)
                return this.Ok("success");

            if (input.Prop1 is not null && !input.Prop2.HasValue)
                return this.Ok("success");

            if (input.Prop1 is null && input.Prop2.HasValue)
                return this.Ok("success");

            return this.Error("Query Succeeded, but shouldn't have. @oneOf was not validated correctly");
        }

        [QueryRoot(typeof(string))]
        public IGraphActionResult SubmitListOfValues(List<InputUnionWithOneOfDirective> inputs)
        {
            if (inputs is null)
                return this.Ok("success");

            var allValid = true;
            foreach (var input in inputs.Where(x => x is not null))
            {
                if (input.Prop1 is not null && !input.Prop2.HasValue)
                    continue;

                if (input.Prop1 is null && input.Prop2.HasValue)
                    continue;

                allValid = false;
            }

            return allValid
                ? this.Ok("success")
                : this.Error("Query Succeeded, but shouldn't have. @oneOf for list elements was not validated correctly");
        }

        [QueryRoot(typeof(string))]
        public IGraphActionResult SubmitSingleValueWithChildUnion(InputObjectWithChildUnion input)
        {
            if (input is null)
                return this.Ok("success");

            var child = input.Child;
            if (child is null)
                return this.Ok("success");

            if (child.Prop1 is not null && !child.Prop2.HasValue)
                return this.Ok("success");

            if (child.Prop1 is null && child.Prop2.HasValue)
                return this.Ok("success");

            return this.Error("Query Succeeded, but shouldn't have. @oneOf was not validated correctly");
        }

        [OneOf]
        [GraphType(InputName = "MyInputUnion")]
        public class InputUnionWithOneOfDirective
        {
            public string Prop1 { get; set; }

            public int? Prop2 { get; set; }
        }


        [GraphType(InputName = "ChildUnionType")]
        public class InputObjectWithChildUnion
        {
            public string Prop0 { get; set; }

            public InputUnionWithOneOfDirective Child { get; set; }
        }
    }
}