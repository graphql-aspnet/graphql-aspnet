// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.UnionTypeExecutionTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class SchoolController : GraphController
    {
        [QueryRoot(typeof(PersonOrTeacher))]
        public IGraphActionResult RetrievePerson(int id)
        {
            object person = null;
            if (id == 1)
            {
                person = new Teacher()
                {
                    Id = id,
                    Name = "Teacher1",
                    NumberOfStudents = 15,
                };
            }
            else if (id == 2)
            {
                person = new Student()
                {
                    Id = id,
                    Name = "Student1",
                    ParentsName = "Bob Smith",
                };
            }
            else if (id == 5)
            {
                person = new TwoPropertyObject()
                {
                    Property1 = "string value",
                    Property2 = id,
                };
            }
            else if (id == 6)
            {
                person = new TwoPropertyObjectV3()
                {
                    Property1 = "string value",
                    Property2 = DateTime.UtcNow,
                };
            }
            else if (id == 7)
            {
                person = new TwoPropertyObjectV2()
                {
                    Property1 = 5,
                    Property2 = DateTime.UtcNow,
                };
            }
            else
            {
                person = new HeadTeacher()
                {
                    Id = id,
                    Name = "HeadMaster",
                    NumberOfStaff = 15,
                    NumberOfStudents = 500,
                };
            }

            return this.Ok(person);
        }
    }
}