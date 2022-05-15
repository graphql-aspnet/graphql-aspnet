namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Schemas;

    [ApplyDirective(typeof(SchemaMarkerDirective))]
    public class MarkedSchema : GraphSchema
    {
    }
}
