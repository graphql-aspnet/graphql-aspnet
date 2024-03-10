// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class SelfReferencingInputObjectController : GraphController
    {
        [QueryRoot]
        public int CountNestings(SelfReferencingInputObject item)
        {
            var i = 0;
            var stack = new Stack<SelfReferencingInputObject>();
            stack.Push(item);
            while (stack.Count > 0)
            {
                var curItem = stack.Pop();
                i++;
                if (curItem.Children != null)
                {
                    foreach (var child in curItem.Children)
                        stack.Push(child);
                }
            }

            return i;
        }
    }
}
