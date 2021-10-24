// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class InputStructController : GraphController
    {
        [QueryRoot]
        public bool ParsePerson(PersonStruct person)
        {
            if (person.FirstName == "Bob" && person.LastName == "Smith")
                return true;

            return false;
        }

        [QueryRoot]
        public bool ParsePersonArray(PersonStruct[] items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].FirstName != $"first{i}" || items[i].LastName != $"last{i}")
                    return false;
            }

            return true;
        }
    }
}