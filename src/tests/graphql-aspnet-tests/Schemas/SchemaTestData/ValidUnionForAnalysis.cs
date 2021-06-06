namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Internal;

    public class ValidUnionForAnalysis : GraphUnionProxy
    {
        public ValidUnionForAnalysis()
            : base(typeof(AddressData), typeof(CountryData))
        {
        }

        public override Type ResolveType(Type runtimeObjectType)
        {
            return typeof(AddressData);
        }
    }
}
