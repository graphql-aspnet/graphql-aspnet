namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(EnumMarkerDirective))]
    public enum MarkedEnum
    {
        Value1,
        Value2,
    }
}
