namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EmployeeData : PersonData
    {
        public string CompanyName { get; set; }
    }
}
